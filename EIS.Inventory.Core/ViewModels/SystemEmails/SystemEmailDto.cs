using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EIS.Inventory.Core.ViewModels
{
   public class SystemEmailDto
    {
        public int Id { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [DisplayName("Email Address")]
        [Required]
        public string EmailAddress { get; set; }

        public bool IsActive { get; set; }
        public string ModifiedBy { get; set; }
    }
    
}
