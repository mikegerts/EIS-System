
using EIS.Inventory.Shared.ViewModels;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace EIS.Inventory.Core.ViewModels
{
    public class MainCustomerDto
    {
        public CustomerDto customerModel { get; set; }
        public CustomerNotesModel customerNotesModel { get; set; }
        public CustomerAddressModel customerAddressModel { get; set; }
        public CustomerWholeSaleModel customerWholeSaleModel { get; set; }
    }

    #region Manage Customer

    public class CustomerDto
    {
        public int CustomerId { get; set; }
        
        [DisplayName("Customer Number")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Accounting Number must be numeric.")]
        public string strCustomerNumber { get; set; }

        public int CustomerNumber { get; set; }

        [Required]
        [DisplayName("Company Name")]
        public string CompanyName { get; set; }

        [DisplayName("Company")]
        public List<SelectListItem> CompanyList { get; set; }
        [Required(ErrorMessage = "company required")]
        public string SelectedCompanyId { get; set; }
        public int CompanyId { get; set; }

        [DisplayName("Firstname")]
        public string FirstName { get; set; }
        [DisplayName("Lastname")]
        public string LastName { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [DisplayName("Email Address")]
        [Required(ErrorMessage = "email address required")]
        public string EmailAddress { get; set; }
        [DisplayName("Office number")]
        public string OfficeNumber { get; set; }

        [DisplayName("Account Type")]
        public List<SelectListItem> AccountTypeList{ get; set; }
        [Required(ErrorMessage ="Account type required")]
        public string SelectedAccountType { get; set; }
        public int AccountType { get; set; }

        [DisplayName("Cost Plus Based Wholesale Price")]
        [RegularExpression(@"^\d+\.\d{0,2}$", ErrorMessage = "Cost must be numeric upto 2 decimal number.")]
        [Range(0, 999.99,ErrorMessage ="Number must be 0 to 999.99")]
        public decimal? CostPlusBasedWholeSalePrice { get; set; }

        [DisplayName("Cost Plus Based Wholesale Price Type")]
        public List<SelectListItem> CostPlusBasedWholeSalePriceTypeList { get; set; }
        public string SelectedCostPlusBasedWholeSalePriceType { get; set; }
        public int? CostPlusBasedWholeSalePriceType { get; set; }


        [DisplayName("Amount Type")]
        public List<SelectListItem> AmountTypeList { get; set; }
        [Required(ErrorMessage = "Amount type required")]
        public string SelectedAmountType { get; set; }
        public int AmountType { get; set; }

        [DisplayName("Credit limit (in dollars)")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Credit limit must be numeric.")]
        public int? CreditLimit { get; set; }

        [DisplayName("Credit Terms (in days)")]
        [RegularExpression("^[0-9]+$", ErrorMessage = "Credit terms must be numeric.")]
        public int? CreditTerms { get; set; }


        public string ModifiedBy { get; set; }
        public string Modified { get; set; }
        public string CreatedBy { get; set; }
        public string Created { get; set; }
    }

    #endregion
    
    #region Customer Notes

    public class CustomerNotesModel {
        public CustomerNotesDto customerNotesDto { get; set; }

        public IEnumerable<CustomerNotesListDto> customerNotesListDto { get; set; }
    }

    public class CustomerNotesDto
    {
        public int CustomerNotesId { get; set; }
        public int NotesCustomerId { get; set; }
        public int CustomerId { get; set; }
        [Required]
        [DisplayName("Title")]
        public string NotesTitle { get; set; }
        [Required]
        [DisplayName("Notes")]
        public string Notes { get; set; }

        public string ModifiedBy { get; set; }
        public string Modified { get; set; }
        public string CreatedBy { get; set; }
        public string Created { get; set; }
    }

    public class CustomerNotesListDto
    {
        public int CustomerNotesId { get; set; }
        public int NotesCustomerId { get; set; }
        public int CustomerId { get; set; }

        public string NotesTitle { get; set; }
        public string Notes { get; set; }

        public string ModifiedBy { get; set; }
        public string Modified { get; set; }
        public string CreatedBy { get; set; }
        public string Created { get; set; }
    }

    #endregion

    #region Customer Address

    public class CustomerAddressModel
    {
        public CustomerAddressDto customerAddressDto { get; set; }

        public IEnumerable<CustomerAddressListDto> customerAddressListDto { get; set; }
    }

    public class CustomerAddressDto
    {
        public int CustomerAddressID { get; set; }
        public int AddressCustomerId { get; set; }
        public int CustomerId { get; set; }

        [Required]
        [DisplayName("Recipient")]
        public string Recipient { get; set; }

        [Required]
        [DisplayName("Company")]
        public string Company { get; set; }

        [Required]
        [DisplayName("Address Line 1")]
        public string AddressLine1 { get; set; }

        [DisplayName("Address Line 2")]
        public string AddressLine2 { get; set; }

        [Required]
        [DisplayName("City")]
        public string City { get; set; }

        [Required]
        [DisplayName("ZipCode")]
        public string ZipCode { get; set; }

        [Required]
        [DisplayName("Phone")]
        public string Phone { get; set; }

        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [DisplayName("Email Address")]
        [Required(ErrorMessage = "email address required")]
        public string customerEmailAddress { get; set; }
        public string EmailAddress { get; set; }


        [DisplayName("Country")]
        public List<SelectListItem> CountryList { get; set; }
        [Required(ErrorMessage = "Country required")]
        public string SelectedCountryId { get; set; }
        public int Country { get; set; }

        [DisplayName("State")]
        [Required(ErrorMessage = "State required")]
        public string State { get; set; }

        [DisplayName("Primary")]
        public bool IsPrimary { get; set; }

        public string ModifiedBy { get; set; }
        public string Modified { get; set; }
        public string CreatedBy { get; set; }
        public string Created { get; set; }
    }

    public class CustomerAddressListDto
    {
        public int CustomerAddressID { get; set; }

        public int AddressCustomerId { get; set; }

        public int CustomerId { get; set; }
        
        public string Recipient { get; set; }

        public string Company { get; set; }

        public string AddressLine1 { get; set; }

        public string AddressLine2 { get; set; }

        public string City { get; set; }

        public string ZipCode { get; set; }

        public string Phone { get; set; }

        public string EmailAddress { get; set; }

        public int CountryId { get; set; }

        public string State { get; set; }

        public bool IsPrimary { get; set; }

        public string ModifiedBy { get; set; }

        public string Modified { get; set; }

        public string CreatedBy { get; set; }

        public string Created { get; set; }
    }

    #endregion

    #region Customer WholeSale

    public class CustomerWholeSaleModel
    {
        public CustomerScheduledTaskDto customerScheduledTaskDto { get; set; }

        public IEnumerable<CustomerScheduledTaskListDto> customerScheduledTaskListDto { get; set; }
    }

    #endregion
}