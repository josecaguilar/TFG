using System;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Net.Http;
using System.Net.Http.Headers;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Security.Authentication.Web;
using Windows.Security.Authentication.Web.Core;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Diagnostics;
using FacialRecognitionDoor.Helpers;
using System.Threading.Tasks;

namespace FacialRecognitionDoor
{
    public sealed partial class Login : Page
    {
        public const string resource = "00000002-0000-0000-c000-000000000000";
        public const string clientId = "731c53ea-3042-4f8d-8da3-a8fcad30fa85";
        public bool ok = false;
        
        public bool PrintCache()
        {
            PrintCache printed = new PrintCache();
            string sub = printed.Cache();
            sub = sub.Substring(sub.IndexOf('@') + 1);
            if (sub == "irondoor.onmicrosoft.com")
            {
                ok = true;
            }
            else
            {
                ok = false;
            }
            return ok;
        }

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            //ClearCache();
            await Search("jlcarrillo", "irondoor.onmicrosoft.com");
            bool ok = PrintCache();
            if (ok)
            {
                Frame.Navigate(typeof(MainPage));
            }
            else
            {
                //User not found
                ForgetPassword.Text = "El usuario no se encuentra en Azure Active Directory";
            }
        }
        public Login()
        {
            this.InitializeComponent();
        }

        public async Task Search(string searchterm, string tenant)
        {
            AuthenticationResult ar = await GetToken(tenant, searchterm);
            if (ar != null)
            {
                JObject jResult = null;

                string graphResourceUri = "https://graph.windows.net";
                string graphApiVersion = "2013-11-08";

                try
                {
                    string graphRequest = String.Format(CultureInfo.InvariantCulture, "{0}/{1}/users?api-version={2}&$filter=mailNickname eq '{3}'", graphResourceUri, ar.TenantId, graphApiVersion, searchterm);
                    HttpClient client = new HttpClient();
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, graphRequest);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", ar.AccessToken);
                    HttpResponseMessage response = await client.SendAsync(request);

                    string content = await response.Content.ReadAsStringAsync();
                    jResult = JObject.Parse(content);
                }
                catch (Exception ee)
                {
                    Debug.WriteLine("Error on search");
                    Debug.WriteLine(ee.Message);
                }

                if (jResult["odata.error"] != null || jResult["value"] == null)
                {
                   Debug.WriteLine("Error on search");
                }
                if (jResult.Count == 0)
                {
                    Debug.WriteLine("No user with alias {0} found. (tenantID: {1})", searchterm, ar.TenantId);
                }
                else
                {
                    /*foreach (JObject result in jResult["value"])
                    {
                        Debug.WriteLine("User found");
                        Debug.WriteLine("-----");
                        Debug.WriteLine("displayName: {0}", (string)result["displayName"]);
                        Debug.WriteLine("givenName: {0}", (string)result["givenName"]);
                        Debug.WriteLine("surname: {0}", (string)result["surname"]);
                        Debug.WriteLine("userPrincipalName: {0}", (string)result["userPrincipalName"]);
                        Debug.WriteLine("telephoneNumber: {0}", (string)result["telephoneNumber"] == null ? "Not Listed." : (string)result["telephoneNumber"]);
                    }*/
                }
            }
            else
            {
                Debug.WriteLine("Failed to obtain a token.");
            }
        }

        public void ClearCache() //static
        {
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            ctx.TokenCache.Clear();
            //Console.ForegroundColor = ConsoleColor.Green;
            Debug.WriteLine("Token cache cleared.");
        }

        public void TestPostMessage(string user, string message)
        {
            string urlWithAccessToken = GeneralConstants.SlackURI;
        
            SlackClient client = new SlackClient(urlWithAccessToken);

            client.PostMessage(username: "IronDoor",
                       text: "IronDoor: "+message,
                       channel: "@"+user);
        }

        public async Task<AuthenticationResult> GetToken(string tenant, string username) //static
        {
            AuthenticationContext ctx = null;
            if (tenant != null)
                ctx = new AuthenticationContext("https://login.microsoftonline.com/" + tenant);
            else
            {
                ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
                if (ctx.TokenCache.Count > 0)
                {
                    string homeTenant = ctx.TokenCache.ReadItems().First().TenantId;
                    ctx = new AuthenticationContext("https://login.microsoftonline.com/" + homeTenant);
                }
            }
            AuthenticationResult result = null;
            try
            {
                result = ctx.AcquireTokenSilentAsync(resource, clientId).Result;
            }
            catch (Exception exc)
            {
                //var adalEx = exc.InnerException as AdalException;
                //if ((adalEx != null) && (adalEx.ErrorCode == "failed_to_acquire_token_silently"))
                if (exc is AdalException || exc.InnerException is AdalException)
                {
                    result = await GetTokenViaCode(ctx, username);
                }
                else
                {
                    Debug.WriteLine("Something went wrong.");
                    Debug.WriteLine("Message: " + exc.InnerException.Message + "\n");
                }
            }
            return result;

        }
        
        public async Task<AuthenticationResult> GetTokenViaCode(AuthenticationContext ctx, string username) //static
        {
            AuthenticationResult result = null;
            try
            {
                DeviceCodeResult codeResult = await ctx.AcquireDeviceCodeAsync(resource, clientId);
                TestPostMessage(username, codeResult.Message);
                //Debug.WriteLine("You need to sign in.");
                //Debug.WriteLine("Message: " + codeResult.Message + "\n");
                result = await ctx.AcquireTokenByDeviceCodeAsync(codeResult);
            }
            catch (Exception exc)
            {
                Debug.WriteLine("Something went wrong.");
                Debug.WriteLine("Message: " + exc.Message + "\n");
            }
            return result;
        }
    }
}
