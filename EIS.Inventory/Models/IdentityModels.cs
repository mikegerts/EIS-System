using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
using MySql.AspNet.Identity;
using System.Linq;

namespace EIS.Inventory.Models
{
    // You can add profile data for the user by adding more properties to your ApplicationUser class, please visit http://go.microsoft.com/fwlink/?LinkID=317594 to learn more.
    public class ApplicationUser : IdentityUser
    {
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<ApplicationUser> manager)
        {
            // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
            var userIdentity = await manager.CreateIdentityAsync(this, DefaultAuthenticationTypes.ApplicationCookie);
            
            // Add custom user claims here
            userIdentity.AddClaim(new Claim("FullName", string.Format("{0} {1}", FirstName, LastName)));
            userIdentity.AddClaim(new Claim("Created", string.Format("{0:MMM yyyy}", Created)));
            try
            {
                userIdentity.AddClaim(new Claim("Role", Roles.FirstOrDefault()));
            }
            catch
            {
                // ignored
            }

            return userIdentity;
        }
    }
}