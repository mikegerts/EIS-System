using System.Configuration;
using System.Web;
using System.Web.Mvc;

namespace EIS.Inventory
{
    public class AuthorizedIPAddressAttribute : ActionFilterAttribute
    {
        private readonly string _whiteListedIPAddresses;

        public AuthorizedIPAddressAttribute()
        {
            _whiteListedIPAddresses = ConfigurationManager.AppSettings["AuthorizeIPAddresses"];
        }

        /// <summary>
        /// Only allows whitelisted IP addresses to access
        /// </summary>
        /// <param name="filterContext"></param>
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // get the user's IP address
            var ipAddress = HttpContext.Current.Request.UserHostAddress;

            if (!isIpAddressWhiteListed(ipAddress.Trim()))
            {
                // send back a HTTP status code of 403 Forbidden
                filterContext.Result = new HttpStatusCodeResult(403);
            }

            base.OnActionExecuting(filterContext);
        }

        /// <summary>
        /// Determine an IP address if it is in the list of whitelisted IP address 
        /// </summary>
        /// <param name="ipAddress">The string representation of the IP address</param>
        /// <returns></returns>
        private bool isIpAddressWhiteListed(string ipAddress)
        {
            // split the user's IP address into its 4 octets, (assumes IPv4)
            var incomingOctets = ipAddress.Split(new char[] { '.' });

            // convert the whitlisted IP address into a string array
            var allowedIPAddresses = _whiteListedIPAddresses.Trim().Split(new char[] { ',' });

            // iterate through each whitelisted IP address
            foreach (var allowedIpAddress in allowedIPAddresses)
            {
                if (allowedIpAddress.Trim() == ipAddress)
                    return true;

                // split the allowed IP into 4 octets
                var allowedOctets = allowedIpAddress.Trim().Split(new char[] { '.' });
                var matches = true;

                // iterate through each octed
                for (var index = 0; index < allowedOctets.Length; index++)
                {
                    // skip if octet is an asterisk indicating an entire subnet range is valid
                    if (allowedOctets[index] != "*")
                    {
                        if (allowedOctets[index] != incomingOctets[index])
                        {
                            matches = false;
                            break; // break out of look
                        }
                    }
                }

                // we found matches, return TRUE
                if (matches)
                    return true;
            }

            // if we reach here, incoming IP address is not authorized
            return false;
        }
    }
}