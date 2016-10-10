using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacialRecognitionDoor
{
    public class PrintCache
    {
        public string Cache()
        {
            AuthenticationContext ctx = new AuthenticationContext("https://login.microsoftonline.com/common");
            var cacheContent = ctx.TokenCache.ReadItems();
            string emailsigned = "";
            
            if (cacheContent.Count() > 0)
            {
                foreach (TokenCacheItem tci in cacheContent)
                {
                    emailsigned = tci.DisplayableId;
                    Debug.WriteLine("{0,-30} | {1,-15}  ", tci.DisplayableId, tci.TenantId);

                }
            }
            else { Debug.WriteLine("The cache is empty."); }

            return emailsigned;
        }
    }


}
