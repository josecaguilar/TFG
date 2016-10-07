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

namespace FacialRecognitionDoor
{

    public sealed partial class Login : Page
    {
        /* ApplicationDataContainer appSettings = null;
         //
         // The Client ID is used by the application to uniquely identify itself to Azure AD.
         const string clientId = "52757930-fd58-4899-bccc-b076c39a1839";

         const string tenant = "josecarrillooutlook.onmicrosoft.com";
         const string authority = "https://login.microsoftonline.com/" + tenant;
         //const string authority = "organizations";

         // To authenticate to the directory Graph, the client needs to know its App ID URI.
         const string resource = "https://graph.windows.net";

         // Windows10 universal apps require redirect URI in the format below
         string URI = string.Format("ms-appx-web://Microsoft.AAD.BrokerPlugIn/{0}", WebAuthenticationBroker.GetCurrentApplicationCallbackUri().Host.ToUpper());

         WebAccountProvider wap = null;
         WebAccount userAccount = null;*/

        public const string resource = "00000002-0000-0000-c000-000000000000";
        public const string clientId = "a887edf6-5e76-43e3-8741-c85d7c3fd974";

        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            Search("jlcarrillo", "josecarrillooutlook.onmicrosoft.com");
        }
        public Login()
        {
            this.InitializeComponent();
            
            /*List<UserSearchResult> results = new List<UserSearchResult>();
            results.Add(new UserSearchResult());
            SearchResults.ItemsSource = results;*/
        }

        static void Search(string searchterm, string tenant)
        {
            AuthenticationResult ar = GetToken(tenant);
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
                    HttpResponseMessage response = client.SendAsync(request).Result;

                    string content = response.Content.ReadAsStringAsync().Result;
                    jResult = JObject.Parse(content);
                }
                catch (Exception ee)
                {
                    //Console.ForegroundColor = ConsoleColor.Red;
                    Debug.WriteLine("Error on search");
                    Debug.WriteLine(ee.Message);
                }

                if (jResult["odata.error"] != null || jResult["value"] == null)
                {
                    //Console.ForegroundColor = ConsoleColor.Red;
                    Debug.WriteLine("Error on search");
                }
                //Console.ForegroundColor = ConsoleColor.Green;
                if (jResult.Count == 0)
                {
                    Debug.WriteLine("No user with alias {0} found. (tenantID: {1})", searchterm, ar.TenantId);
                }
                else
                {
                    Debug.WriteLine("Users found.");
                    foreach (JObject result in jResult["value"])
                    {
                        Debug.WriteLine("-----");
                        Debug.WriteLine("displayName: {0}", (string)result["displayName"]);
                        Debug.WriteLine("givenName: {0}", (string)result["givenName"]);
                        Debug.WriteLine("surname: {0}", (string)result["surname"]);
                        Debug.WriteLine("userPrincipalName: {0}", (string)result["userPrincipalName"]);
                        Debug.WriteLine("telephoneNumber: {0}", (string)result["telephoneNumber"] == null ? "Not Listed." : (string)result["telephoneNumber"]);
                    }
                }
            }
            else
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                Debug.WriteLine("Failed to obtain a token.");
            }
        }

        static void ClearCache()
        {
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            ctx.TokenCache.Clear();
            //Console.ForegroundColor = ConsoleColor.Green;
            Debug.WriteLine("Token cache cleared.");
        }

        static AuthenticationResult GetToken(string tenant)
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
                var adalEx = exc.InnerException as AdalException;
                if ((adalEx != null) && (adalEx.ErrorCode == "failed_to_acquire_token_silently"))
                {
                    result = GetTokenViaCode(ctx);
                }
                else
                {
                    //Console.ForegroundColor = ConsoleColor.Red;
                    Debug.WriteLine("Something went wrong.");
                    Debug.WriteLine("Message: " + exc.InnerException.Message + "\n");
                }
            }
            return result;

        }
        static AuthenticationResult GetTokenViaCode(AuthenticationContext ctx)
        {
            AuthenticationResult result = null;
            try
            {
                DeviceCodeResult codeResult = ctx.AcquireDeviceCodeAsync(resource, clientId).Result;
                //Console.ResetColor();
                Debug.WriteLine("You need to sign in.");
                Debug.WriteLine("Message: " + codeResult.Message + "\n");
                result = ctx.AcquireTokenByDeviceCodeAsync(codeResult).Result;
            }
            catch (Exception exc)
            {
                //Console.ForegroundColor = ConsoleColor.Red;
                Debug.WriteLine("Something went wrong.");
                Debug.WriteLine("Message: " + exc.Message + "\n");
            }
            return result;
        }

        /*
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            wap = await WebAuthenticationCoreManager.FindAccountProviderAsync("https://login.microsoft.com", authority);
            appSettings = ApplicationData.Current.RoamingSettings;                               
            WebTokenRequest wtr = new WebTokenRequest(wap, string.Empty, clientId);
            wtr.Properties.Add("resource", resource);
            
            // Check if there's a record of the last account used with the app
            var userID = appSettings.Values["userID"];
            if (userID != null)
            {
                // Get an account object for the user
                userAccount = await WebAuthenticationCoreManager.FindAccountAsync(wap, (string)userID);
                if (userAccount != null)
                {
                    // Ensure that the saved account works for getting the token we need
                    WebTokenRequestResult wtrr = await WebAuthenticationCoreManager.RequestTokenAsync(wtr, userAccount);
                    if (wtrr.ResponseStatus == WebTokenRequestStatus.Success)
                    {                        
                        userAccount = wtrr.ResponseData[0].WebAccount;
                    }
                    else
                    {                        
                        // The saved account could not be used for getting a token
                        ContentDialog messageDialog = new ContentDialog("We tried to sign you in with the last account you used with this app, but it didn't work out. Please sign in as a different user.");
                        await messageDialog.ShowAsync();
                        // Make sure that the UX is ready for a new sign in
                        UpdateUXonSignOut();
                    }
                }
                else
                {
                    // The WebAccount object is no longer available. Let's attempt a sign in with the saved username
                    wtr.Properties.Add("LoginHint", appSettings.Values["login_hint"].ToString());
                    WebTokenRequestResult wtrr = await WebAuthenticationCoreManager.RequestTokenAsync(wtr);
                    if (wtrr.ResponseStatus == WebTokenRequestStatus.Success)
                    {                        
                        userAccount = wtrr.ResponseData[0].WebAccount;
                    }
                }
            }
            else
            {  
                // There is no recorded user. Let's start a sign in flow without imposing a specific account.                             
                WebTokenRequestResult wtrr = await WebAuthenticationCoreManager.RequestTokenAsync(wtr);
                if (wtrr.ResponseStatus == WebTokenRequestStatus.Success)
                {
                    userAccount = wtrr.ResponseData[0].WebAccount;
                }
            }

            if (userAccount != null) // we succeeded in obtaining a valid user
            {
                // save user ID in local storage
                UpdateUXonSignIn();
            }
            else
            {
                // nothing we tried worked. Ensure that the UX reflects that there is no user currently signed in.
                UpdateUXonSignOut();
                MessageDialog messageDialog = new MessageDialog("We could not sign you in. Please try again.");
                await messageDialog.ShowAsync();                
            }
        }


        // perform a user search by alias against the directory Graph of the currently signed in user
        private async void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            // craft the token request for the Graph api
            WebTokenRequest wtr = new WebTokenRequest(wap, string.Empty, clientId);
            wtr.Properties.Add("resource", resource);
            // perform the token request without showing any UX
            WebTokenRequestResult wtrr = await WebAuthenticationCoreManager.GetTokenSilentlyAsync(wtr, userAccount);
            if (wtrr.ResponseStatus == WebTokenRequestStatus.Success)
            {
                string accessToken = wtrr.ResponseData[0].Token;
                try
                {
                    SearchResults.ItemsSource = await DirectorySearcher.SearchByAlias(SearchTermText.Text, accessToken, userAccount.Properties["TenantId"]);
                }
                catch (Exception ee)
                {
                    MessageDialog messageDialog = new MessageDialog("The Graph query didn't work. Error: "+ee.Message);
                    await messageDialog.ShowAsync();
                }
            }
            else
            {
                MessageDialog messageDialog = new MessageDialog("We tried to get a token for the Graph as the account you are currently signed in, but it didn't work out. Please sign in as a different user.");
                await messageDialog.ShowAsync();
            }            
        }

        // Change the currently signed in user
        private async void btnSignInOut_Click(object sender, RoutedEventArgs e)
        {
            // prepare a request with 'WebTokenRequestPromptType.ForceAuthentication', 
            // which guarantees that the user will be able to enter an account of their choosing
            // regardless of what accounts are already present on the system
            WebTokenRequest wtr = new WebTokenRequest(wap, string.Empty, clientId, WebTokenRequestPromptType.ForceAuthentication);
            wtr.Properties.Add("resource", resource);
            WebTokenRequestResult wtrr = await WebAuthenticationCoreManager.RequestTokenAsync(wtr);
            if (wtrr.ResponseStatus == WebTokenRequestStatus.Success)
            {
                userAccount = wtrr.ResponseData[0].WebAccount;
                UpdateUXonSignIn();
            }
            else
            {
                UpdateUXonSignOut();
                MessageDialog messageDialog = new MessageDialog("We could not sign you in. Please try again.");
                await messageDialog.ShowAsync();
            }
        }
        // update the UX and the app settings to show that a user is signed in
        private void UpdateUXonSignIn()
        {
            appSettings.Values["userID"] = userAccount.Id;
            appSettings.Values["login_hint"] = userAccount.UserName;
            textSignedIn.Text = string.Format("you are signed in as {0} - ", userAccount.UserName);
            btnSignInOut.Content = "Sign in as a different user";
            btnSearch.IsEnabled = true;
        }

        // update the UX and the app settings to show that no user is signed in at the moment
        private void UpdateUXonSignOut()
        {
                appSettings.Values["userID"] = null;
                appSettings.Values["login_hint"] = null;
            btnSearch.IsEnabled = false;
                textSignedIn.Text = "You are not signed in. ";
                btnSignInOut.Content = "Sign in";
                SearchResults.ItemsSource = new List<UserSearchResult>();
        }     */
    }
}
