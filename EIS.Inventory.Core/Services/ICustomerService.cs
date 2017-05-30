using System;
using System.Collections.Generic;
using X.PagedList;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Models;
using System.Web.Mvc;

namespace EIS.Inventory.Core.Services
{
    public interface ICustomerService : IDisposable
    {
        #region Manage Customer
        /// <summary>
        /// Get the paged list of the customer
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="searchString">The keyword to search for customer's details</param>
        /// <returns></returns>
        IPagedList<CustomerListDto> GetPagedCustomers(int page,
            int pageSize,
            string searchString, int CustomerNumber,
            string CompanyName, string CustomerName, string EmailAddress, int CompanyId, int AccountTypeId);

        /// <summary>
        /// Get ther Customers with the specified Customers id
        /// </summary>
        /// <param name="CustomerId">The Customers Id</param>
        /// <returns></returns>
        CustomerDto GetCustomer(int CustomerId);

        /// <summary>
        /// Create new Customer in the database
        /// </summary>
        /// <param name="model">The Customer to save</param>
        /// <returns></returns>
        CustomerDto CreateCustomer(CustomerDto model);

        /// <summary>
        /// Update the Customer with the modified model
        /// </summary>
        /// <param name="model">The updated Customer</param>
        /// <returns></returns>
        CustomerDto UpdateCustomer(CustomerDto model);

        /// <summary>
        /// Delete the Customer with the specified Customer Id
        /// </summary>
        /// <param name="Id">The Customer Id</param>
        /// <returns></reCustomerturns>
        bool DeleteCustomer(int Id);


        /// <summary>
        /// Check the Customer emailaddress
        /// </summary>
        /// <param name="CustomerId">The Customer Id</param>
        /// <param name="emailAddress">The email address</param>
        /// <returns></returns>
        bool IsEmailExist(int customerId, string emailAddress);


        #endregion

        #region Customer Documents
        /// <summary>
        /// Get the list of customer's images with the specified customer Id
        /// </summary>
        /// <param name="customerId">The customerId</param>
        /// <returns></returns>
        IEnumerable<MediaContent> GetCustomerFiles(int customerId);

        /// <summary>
        /// Get the customer image with the specified customer image id
        /// </summary>
        /// <param name="id">The id of the image</param>
        /// <returns></returns>
        MediaContent GetCustomerFile(long id);

        /// <summary>
        /// Delete the customer image with the specified id
        /// </summary>
        /// <param name="id">The id of the customer image</param>
        /// <param name="customerId">The customreId</param>
        /// <returns></returns>
        bool DeleteCustomerFile(long id);

        /// <summary>
        /// Update the customer images and delete the old Amazon images
        /// </summary>
        /// <param name="imageUrls">The list of image URLs</param>
        /// <param name="customerId">The customerId</param>
        void UpdateCustomerFiles(List<MediaContent> imageUrls, int customerId);

        /// <summary>
        /// Save the image to the database
        /// </summary>
        /// <param name="image"></param>
        void AddCustomerFile(MediaContent image);

        /// <summary>
        /// Update the image details
        /// </summary>
        /// <param name="id">The id of the image</param>
        /// <param name="fileName">The filename of the image</param>
        /// <param name="caption">The caption for the image</param>
        void UpdateCustomerFile(long id, string fileName, string caption);

        #endregion

        #region Customer Notes
        /// <summary>
        /// Get ther Customers Notes with the specified Customers id
        /// </summary>
        /// <param name="CustomerId">The Customers Id</param>
        /// <returns></returns>
        IEnumerable<CustomerNotesListDto> GetCustomerNotesList(int CustomerId);

        /// <summary>
        /// Get ther Customers Notes with the specified Customers id
        /// </summary>
        /// <param name="CustomerId">The Customers Id</param>
        /// <returns></returns>
        CustomerNotesDto GetCustomerNotes(int CustomerId);

        /// <summary>
        /// Create new Customer Notes in the database
        /// </summary>
        /// <param name="model">The Customer Notes to save</param>
        /// <returns></returns>
        CustomerNotesDto CreateCustomerNotes(CustomerNotesDto model);

        /// <summary>
        /// Update the Customer Notes with the modified model
        /// </summary>
        /// <param name="model">The updated Customer Notes</param>
        /// <returns></returns>
        CustomerNotesDto UpdateCustomerNotes(CustomerNotesDto model);


        /// <summary>
        /// Delete the customer notes with the specified id
        /// </summary>
        /// <param name="id">The id of the customer notes </param>
        /// <returns></returns>
        bool DeleteCustomerNotes(long id);

        #endregion

        #region Customer Address
        /// <summary>
        /// Get ther Customers Address with the specified Customers id
        /// </summary>
        /// <param name="CustomerId">The Customers Id</param>
        /// <returns></returns>
        IEnumerable<CustomerAddressListDto> GetCustomerAddressList(int CustomerId);

        /// <summary>
        /// Get ther Customers Address with the specified Customers id
        /// </summary>
        /// <param name="CustomerId">The Customers Id</param>
        /// <returns></returns>
        CustomerAddressDto GetCustomerAddress(int CustomerAddressId);

        /// <summary>
        /// Create new Customer Address in the database
        /// </summary>
        /// <param name="model">The Customer Address to save</param>
        /// <returns></returns>
        CustomerAddressDto CreateCustomerAddress(CustomerAddressDto model);

        /// <summary>
        /// Update the Customer Address with the modified model
        /// </summary>
        /// <param name="model">The updated Customer Address</param>
        /// <returns></returns>
        CustomerAddressDto UpdateCustomerAddress(CustomerAddressDto model);


        /// <summary>
        /// Delete the customer Address with the specified id
        /// </summary>
        /// <param name="id">The id of the customer Address </param>
        /// <returns></returns>
        bool DeleteCustomerAddress(long id);

        List<SelectListItem> GetCountryList();
        #endregion

        #region WholeSale Price History

        /// <summary>
        /// Get the Customer WholeSale Price History with the specified eisSku
        /// </summary>
        /// <param name="eisSku">The eisSku</param>
        /// <returns></returns>
        IEnumerable<CustomerWholeSalePriceHistoryDto> GetCustomerWholeSalePriceHistorysList(string eisSku);

        #endregion
    }
}