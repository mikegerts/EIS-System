using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.ViewModels {

    public class CompanyDto {

        public int Id { get; set; }
        [DisplayName("Company Name")]
        [Required]
        public string Name { get; set; }
        public string Currency { get; set; }
        public string Abbreviation { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Phone { get; set; }
        [Required]
        public string SKUCodeStart { get; set; }
        [Required]
        public string SearialSKUCode { get; set; }
        public int IsDefault { get; set; }
        public string ModifiedBy { get; set; }
    }

}
