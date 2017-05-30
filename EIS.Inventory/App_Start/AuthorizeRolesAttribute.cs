using System.Linq;
using System.Security.Claims;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace EIS.Inventory
{
    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params string[] roles)
            : base()
        {
            Roles = string.Join(",", roles);

        }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            // no user is authenticated, no need to go any further
            if (!httpContext.User.Identity.IsAuthenticated)
                return false;

            // get the user's roles, if no roles for the user, he's not authorized!
            var identity = (ClaimsIdentity)httpContext.User.Identity;
            var userRoles = identity.Claims.Where(x => x.Type == ClaimTypes.Role).Select(x => x.Value);

            var roles = Roles.Split(',');
            foreach (var role in roles)
            {
                if (userRoles.Any(r => r == role))
                    return true;
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.Request.IsAuthenticated)
                base.HandleUnauthorizedRequest(filterContext);
            else
            {
                // redirect to 401 page
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary {
                    {"action", "accessdenied" },
                    {"controller", "account"}
                });
            }
        }
    }
}