using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;

namespace EIS.Inventory.Helpers
{
    public static class PrincipalExtensions
    {
        public static string GetFullName(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst("FullName");

            return claim == null ? string.Empty : claim.Value;
        }

        public static string GetMemberSince(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst("Created");

            return claim == null ? string.Empty : claim.Value;
        }

        public static string GetRole(this IPrincipal user)
        {
            var claim = ((ClaimsIdentity)user.Identity).FindFirst("Role");

            return claim == null ? string.Empty : claim.Value;
        }
    }
}