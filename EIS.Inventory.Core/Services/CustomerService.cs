using System;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;
using AutoMapper;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using System.Data.Entity.Validation;
using EIS.Inventory.Shared.Helpers;
using System.IO;
using System.Web.Mvc;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly EisInventoryContext _context;
        private readonly ILogService _logger;
        private readonly IFileHelper _fileHelper;
        private const string _subFolderName = "Customer";

        public CustomerService(IFileHelper fileHelper, ILogService logger)
        {
            _fileHelper = fileHelper;
            _logger = logger;
            _context = new EisInventoryContext();
        }

        #region Manage Customer

        public CustomerDto CreateCustomer(CustomerDto model)
        {
            try
            {
                model.CompanyId = int.Parse(model.SelectedCompanyId);
                model.AccountType = int.Parse(model.SelectedAccountType);
                model.AmountType = int.Parse(model.SelectedAmountType);

                if (!string.IsNullOrEmpty(model.SelectedCostPlusBasedWholeSalePriceType))
                    model.CostPlusBasedWholeSalePriceType = int.Parse(model.SelectedCostPlusBasedWholeSalePriceType);


                if (!string.IsNullOrEmpty(model.strCustomerNumber))
                    model.CustomerNumber = int.Parse(model.strCustomerNumber);
                else
                    model.CustomerNumber = getMaxCustomerNumber() + 1;

                var customer = Mapper.Map<customer>(model);
                customer.Created = DateTime.UtcNow;
                customer.CreatedBy = model.ModifiedBy;
                customer.ModifiedBy = null;

                _context.customers.Add(customer);
                _context.SaveChanges();
                model.CustomerId = customer.CustomerId;

                return model;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.CustomerService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.CustomerService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        public bool DeleteCustomer(int Id)
        {
            var customer = _context.customers.SingleOrDefault(x => x.CustomerId == Id);
            if (customer == null)
                return false;

            var customerImages = _context.customerimages.Where(x => x.CustomerId == Id).ToList();

            foreach (var image in customerImages)
            {
                _context.customerimages.Remove(image);
                _context.SaveChanges();
            }

            var customerAddresses = _context.customersaddresses.Where(x => x.CustomerId == Id).ToList();

            foreach (var address in customerAddresses)
            {
                _context.customersaddresses.Remove(address);
                _context.SaveChanges();
            }

            var customerNotes = _context.customersnotes.Where(x => x.CustomerId == Id).ToList();

            foreach (var note in customerNotes)
            {
                _context.customersnotes.Remove(note);
                _context.SaveChanges();
            }

            var customerSchedules = _context.customerscheduledtasks.Where(x => x.CustomerId == Id).ToList();

            foreach (var schedule in customerSchedules)
            {
                var customerExportFiles = _context.customerexportedfiles.Where(x => x.ScheduledTaskId == schedule.Id).ToList();

                foreach (var exportFile in customerExportFiles)
                {
                    _context.customerexportedfiles.Remove(exportFile);
                    _context.SaveChanges();
                }
                _context.customerscheduledtasks.Remove(schedule);
                _context.SaveChanges();
            }



            _context.customers.Remove(customer);
            _context.SaveChanges();

            return true;
        }


        public CustomerDto GetCustomer(int CustomerId)
        {
            var customer = _context.customers.FirstOrDefault(x => x.CustomerId == CustomerId);

            return Mapper.Map<customer, CustomerDto>(customer);
        }

        public IPagedList<CustomerListDto> GetPagedCustomers(int page, int pageSize, string searchString, int CustomerNumber,
            string CompanyName, string CustomerName, string EmailAddress, int CompanyId, int AccountTypeId)
        {
            return _context.customers.Where(x => (string.IsNullOrEmpty(searchString)
                            || x.EmailAddress.Contains(searchString)
                            || x.FirstName.Contains(searchString)
                            || x.LastName.Contains(searchString)
                            || x.OfficeNumber.Contains(searchString)
                            || x.CompanyName.Contains(searchString))
                            && (CustomerNumber == 0 || x.CustomerNumber == CustomerNumber)
                            && (string.IsNullOrEmpty(CompanyName) || x.CompanyName.Contains(CompanyName))
                             && (string.IsNullOrEmpty(CustomerName) || (x.FirstName.Contains(CustomerName) || x.LastName.Contains(CustomerName)))
                              && (string.IsNullOrEmpty(EmailAddress) || x.CompanyName.Contains(EmailAddress))
                              && (CompanyId == -1 || x.CompanyId == CompanyId)
                              && (AccountTypeId == -1 || x.AccountType == AccountTypeId))
                .OrderBy(x => x.CustomerId)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<customer, CustomerListDto>();
        }

        public bool IsEmailExist(int customerId, string emailAddress)
        {
            if (customerId > 0)
            {
                return _context.customers.Any(x => x.EmailAddress == emailAddress && x.CustomerId != customerId);
            }
            else
            {
                return _context.customers.Any(x => x.EmailAddress == emailAddress);
            }
        }

        public CustomerDto UpdateCustomer(CustomerDto model)
        {
            try
            {

                model.CompanyId = int.Parse(model.SelectedCompanyId);
                model.AccountType = int.Parse(model.SelectedAccountType);
                model.AmountType = int.Parse(model.SelectedAmountType);

                if (!string.IsNullOrEmpty(model.SelectedCostPlusBasedWholeSalePriceType))
                    model.CostPlusBasedWholeSalePriceType = int.Parse(model.SelectedCostPlusBasedWholeSalePriceType);

                var oldCustomer = _context.customers.FirstOrDefault(x => x.CustomerId == model.CustomerId);
                model.CustomerNumber = oldCustomer.CustomerNumber;

                var updatedCustomer = Mapper.Map<customer>(model);
                updatedCustomer.Modified = DateTime.Now;
                updatedCustomer.ModifiedBy = model.ModifiedBy;
                updatedCustomer.Created = oldCustomer.Created;
                updatedCustomer.CreatedBy = oldCustomer.CreatedBy;



                _context.Entry(oldCustomer).CurrentValues.SetValues(updatedCustomer);
                _context.SaveChanges();

                return model;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.CustomerService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.CustomerService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        private int getMaxCustomerNumber()
        {
            int maxEisCustomerNumber = 1;
            using (var context = new EisInventoryContext())
            {
                try
                {
                    var existingMaxCustomerNumber = context.customers.Max(x => x.CustomerNumber);
                    maxEisCustomerNumber = existingMaxCustomerNumber > 0 ? existingMaxCustomerNumber : maxEisCustomerNumber;
                }
                catch { }
            }
            return maxEisCustomerNumber;
        }


        public IEnumerable<CustomerWholeSalePriceHistoryDto> GetCustomerWholeSalePriceHistorysList(string eisSku)
        {
            var customerWholeSalePriceHistoryList = _context.customerwholesalepricehistories.Where(x => x.EisSKU == eisSku).ToList();
            return Mapper.Map<IEnumerable<customerwholesalepricehistory>, IEnumerable<CustomerWholeSalePriceHistoryDto>>(customerWholeSalePriceHistoryList);
        }


        #endregion


        #region Customer Documents

        public IEnumerable<MediaContent> GetCustomerFiles(int customerId)
        {
            var customerImages = _context.customerimages
               .Where(x => x.CustomerId == customerId);

            var images = new List<MediaContent>();
            foreach (var item in customerImages)
            {
                images.Add(new MediaContent
                {
                    Id = item.Id,
                    ParentId = item.CustomerId.ToString(),
                    Url = _fileHelper.GetFileUri(_subFolderName, customerId.ToString(), item.FileName),
                    Caption = item.Caption,
                });
            }

            return images;
        }

        public MediaContent GetCustomerFile(long id)
        {
            var image = _context.customerimages.FirstOrDefault(x => x.Id == id);
            if (image == null) return null;

            return new MediaContent
            {
                Id = image.Id,
                ParentId = image.CustomerId.ToString(),
                Url = _fileHelper.GetFileUri(_subFolderName, image.CustomerId.ToString(), image.FileName),
                Caption = image.Caption,
            };

        }

        public bool DeleteCustomerFile(long id)
        {
            var image = _context.customerimages.FirstOrDefault(x => x.Id == id);
            if (image == null)
                return true;

            // delete the image from database first
            var customerId = image.CustomerId;
            _context.customerimages.Remove(image);
            _context.SaveChanges();

            // then the file
            _fileHelper.RemoveFile(_subFolderName, customerId.ToString(), image.FileName);

            return true;
        }

        public void UpdateCustomerFiles(List<MediaContent> imageUrls, int customerId)
        {
            try
            {
                // get the Customer images
                var CustomerImages = _context.customerimages
                    .Where(x => x.CustomerId == customerId)
                    .ToList();
                foreach (var image in CustomerImages)
                {
                    // delete first the image file from the directory
                    _fileHelper.RemoveFile(_subFolderName, customerId.ToString(), image.FileName);

                    // then the image record from the database
                    _context.customerimages.Remove(image);
                }

                // let's save the changes first for the deleted images
                _context.SaveChanges();


                foreach (var media in imageUrls)
                {
                    // download the image from net and save it to the file system
                    var extension = Path.GetExtension(media.Url);
                    var fileName = _fileHelper.SaveFile(_subFolderName, customerId.ToString(), extension, media.Url);
                    if (string.IsNullOrEmpty(fileName))
                        continue;

                    // add the database to the database
                    addCustomerFile(fileName, customerId, 99, "Customer Large Image", media.Type);
                }

                // save the images
                _context.SaveChanges();

            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.CustomerService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }
        }
        public void UpdateCustomerFiles(List<string> imageUrls, int customerId)
        {
            // check if there's any image URLs
            if (imageUrls == null || !imageUrls.Any())
                return;

            try
            {
                using (var context = new EisInventoryContext())
                {
                    // get the Customer images
                    var CustomerImages = context.customerimages
                        .Where(x => x.CustomerId == customerId)
                        .ToList();
                    foreach (var image in CustomerImages)
                    {
                        // delete first the image file from the directory
                        _fileHelper.RemoveFile(_subFolderName, customerId.ToString(), image.FileName);

                        // then the image record from the database
                        context.customerimages.Remove(image);
                    }

                    // let's save the changes first for the deleted images
                    context.SaveChanges();

                    // download and add the images
                    foreach (var url in imageUrls)
                    {
                        // download the image from net and save it to the file system
                        var extension = Path.GetExtension(url);
                        var fileName = _fileHelper.SaveFile(_subFolderName, customerId.ToString(), extension, url);
                        if (string.IsNullOrEmpty(fileName))
                            continue;

                        // add the database to the database
                        context.customerimages.Add(new customerimage
                        {
                            CustomerId = customerId,
                            Caption = "Customer",
                            FileName = fileName,
                            Order_ = 99
                        });
                    }

                    // save the images
                    context.SaveChanges();
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.CustomerService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
            }
        }

        public void AddCustomerFile(MediaContent image)
        {
            addCustomerFile(image.Url, int.Parse(image.ParentId), 99, image.Caption, image.Type);
        }

        public void UpdateCustomerFile(long id, string fileName, string caption)
        {
            var image = _context.customerimages.FirstOrDefault(x => x.Id == id);
            if (image == null)
                return;

            image.FileName = fileName;
            image.Caption = caption;

            _context.SaveChanges();
        }

        private void addCustomerFile(string fileName, int customerId, int order, string caption, string imageType, bool isPush = false)
        {
            var customerImage = new customerimage
            {
                CustomerId = customerId,
                Caption = caption,
                FileName = fileName,
                Order_ = order
            };

            _context.customerimages.Add(customerImage);

            if (!isPush)
                _context.SaveChanges();
        }


        #endregion


        #region Customer Notes


        public IEnumerable<CustomerNotesListDto> GetCustomerNotesList(int CustomerId)
        {
            var customerNotesList = _context.customersnotes.Where(x => x.CustomerId == CustomerId).ToList();
            return Mapper.Map<IEnumerable<customersnote>, IEnumerable<CustomerNotesListDto>>(customerNotesList);
        }

        public CustomerNotesDto CreateCustomerNotes(CustomerNotesDto model)
        {
            try
            {
                var customerNotes = Mapper.Map<customersnote>(model);
                customerNotes.Created = DateTime.UtcNow;
                customerNotes.CreatedBy = model.ModifiedBy;
                customerNotes.ModifiedBy = null;
                customerNotes.CustomerId = model.NotesCustomerId;

                _context.customersnotes.Add(customerNotes);
                _context.SaveChanges();
                model.CustomerNotesId = customerNotes.CustomerNotesId;

                return model;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.CustomerService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.CustomerService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        public CustomerNotesDto UpdateCustomerNotes(CustomerNotesDto model)
        {
            try
            {
                var oldCustomerNotes = _context.customersnotes.FirstOrDefault(x => x.CustomerNotesId == model.CustomerNotesId);

                var updatedCustomerNotes = Mapper.Map<customersnote>(model);
                updatedCustomerNotes.Modified = DateTime.Now;
                updatedCustomerNotes.ModifiedBy = model.ModifiedBy;
                updatedCustomerNotes.Created = oldCustomerNotes.Created;
                updatedCustomerNotes.CreatedBy = oldCustomerNotes.CreatedBy;
                updatedCustomerNotes.CustomerId = model.NotesCustomerId;


                _context.Entry(oldCustomerNotes).CurrentValues.SetValues(updatedCustomerNotes);
                _context.SaveChanges();

                return model;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.CustomerService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.CustomerService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        public CustomerNotesDto GetCustomerNotes(int CustomerNotesId)
        {
            var customerNotes = _context.customersnotes.FirstOrDefault(x => x.CustomerNotesId == CustomerNotesId);
            return Mapper.Map<customersnote, CustomerNotesDto>(customerNotes);
        }

        public bool DeleteCustomerNotes(long id)
        {
            var customerNotes = _context.customersnotes.SingleOrDefault(x => x.CustomerNotesId == id);
            if (customerNotes == null)
                return false;

            _context.customersnotes.Remove(customerNotes);
            _context.SaveChanges();

            return true;
        }

        #endregion


        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _logger.Dispose();
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }

        #endregion

        #region Customer Address


        public IEnumerable<CustomerAddressListDto> GetCustomerAddressList(int CustomerId)
        {
            var customerAddressesList = _context.customersaddresses.Where(x => x.CustomerId == CustomerId).ToList();
            return Mapper.Map<IEnumerable<customersaddress>, IEnumerable<CustomerAddressListDto>>(customerAddressesList);
        }

        public CustomerAddressDto GetCustomerAddress(int CustomerAddressId)
        {
            var customerAddress = _context.customersaddresses.FirstOrDefault(x => x.CustomerAddressID == CustomerAddressId);
            return Mapper.Map<customersaddress, CustomerAddressDto>(customerAddress);
        }

        public CustomerAddressDto CreateCustomerAddress(CustomerAddressDto model)
        {
            try
            {
                var customerAddressesList = _context.customersaddresses.Where(x => x.CustomerId == model.AddressCustomerId).ToList();

                if (customerAddressesList.Count == 0 || !customerAddressesList.Any(x => x.IsPrimary == true))
                {
                    model.IsPrimary = true;
                }
                else if (model.IsPrimary)
                {
                    foreach (var address in customerAddressesList)
                    {
                        address.IsPrimary = false;

                        _context.Entry(address).State = System.Data.Entity.EntityState.Modified;
                        _context.SaveChanges();
                    }
                }

                var customerAddress = Mapper.Map<customersaddress>(model);
                customerAddress.Created = DateTime.UtcNow;
                customerAddress.CreatedBy = model.ModifiedBy;
                customerAddress.ModifiedBy = null;
                customerAddress.CustomerId = model.AddressCustomerId;
                customerAddress.EmailAddress = model.customerEmailAddress;
                customerAddress.Country = Convert.ToInt32(model.SelectedCountryId);

                _context.customersaddresses.Add(customerAddress);
                _context.SaveChanges();
                model.CustomerAddressID = customerAddress.CustomerAddressID;

                return model;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.CustomerService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.CustomerService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        public CustomerAddressDto UpdateCustomerAddress(CustomerAddressDto model)
        {
            try
            {

                var customerAddressesList = _context.customersaddresses.Where(x => x.CustomerId == model.AddressCustomerId).ToList();

                if (customerAddressesList.Count == 0 || !customerAddressesList.Any(x => x.IsPrimary == true))
                {
                    model.IsPrimary = true;
                }
                else if (model.IsPrimary)
                {
                    foreach (var address in customerAddressesList)
                    {
                        address.IsPrimary = false;

                        _context.Entry(address).State = System.Data.Entity.EntityState.Modified;
                        _context.SaveChanges();
                    }
                }

                var oldCustomerAddress = _context.customersaddresses.FirstOrDefault(x => x.CustomerAddressID == model.CustomerAddressID);

                var updatedCustomerAddress = Mapper.Map<customersaddress>(model);
                updatedCustomerAddress.Modified = DateTime.Now;
                updatedCustomerAddress.ModifiedBy = model.ModifiedBy;
                updatedCustomerAddress.Created = oldCustomerAddress.Created;
                updatedCustomerAddress.CreatedBy = oldCustomerAddress.CreatedBy;

                updatedCustomerAddress.CustomerId = model.AddressCustomerId;
                updatedCustomerAddress.EmailAddress = model.customerEmailAddress;
                updatedCustomerAddress.Country = Convert.ToInt32(model.SelectedCountryId);


                _context.Entry(oldCustomerAddress).CurrentValues.SetValues(updatedCustomerAddress);
                _context.SaveChanges();

                return model;
            }
            catch (DbEntityValidationException ex)
            {
                var errorMsg = EisHelper.ParseDbEntityValidationException(ex);
                _logger.LogError(LogEntryType.CustomerService, errorMsg, ex.StackTrace);
                throw ex;
            }
            catch (Exception ex)
            {
                _logger.LogError(LogEntryType.CustomerService, EisHelper.GetExceptionMessage(ex), ex.StackTrace);
                throw ex;
            }
        }

        public bool DeleteCustomerAddress(long id)
        {
            bool isPrimary = false;
            int customerId = 0;
            var customerAddress = _context.customersaddresses.SingleOrDefault(x => x.CustomerAddressID == id);
            if (customerAddress == null)
                return false;

            if (customerAddress.IsPrimary == true)
            {
                isPrimary = true;
                customerId = customerAddress.CustomerId;
            }

            _context.customersaddresses.Remove(customerAddress);
            _context.SaveChanges();

            if (isPrimary)
            {
                var customerAddressesList = _context.customersaddresses.Where(x => x.CustomerId == customerId).ToList();
                if (customerAddressesList.Count > 0)
                {
                    var firstAddress = customerAddressesList.First();

                    firstAddress.IsPrimary = true;
                    _context.Entry(firstAddress).State = System.Data.Entity.EntityState.Modified;
                    _context.SaveChanges();
                }
            }

            return true;
        }

        public List<SelectListItem> GetCountryList()
        {
            return _context.countries.Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();
        }

        #endregion
    }
}