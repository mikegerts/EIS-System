using System;
using System.Linq;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.Shared.Helpers;
using EIS.Inventory.Shared.Models;
using System.IO;
using EIS.Inventory.Shared.ViewModels;
using System.Configuration;
using System.Web;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class CustomerController : Controller
    {
        private readonly ICustomerService _customerService;
        private readonly ICompanyService _companyService;
        private readonly ISavedSearchFilterService _savedSearchFilterService;
        private readonly ICustomerScheduledTaskService _customerScheduledTaskService;
        private readonly IFileHelper _fileHelper;
        private const string _subFolderName = "Customer";

        public CustomerController(ICustomerService customerService, ICompanyService companyService, IFileHelper fileHelper,
            ISavedSearchFilterService savedSearchFilterService, ICustomerScheduledTaskService customerScheduledTaskService)
        {
            _customerService = customerService;
            _companyService = companyService;
            _savedSearchFilterService = savedSearchFilterService;
            _fileHelper = fileHelper;
            _customerScheduledTaskService = customerScheduledTaskService;
        }


        [AuthorizeRoles("Administrator", "CanViewCustomer")]
        public ActionResult Index(string searchString, int page = 1, int pageSize = 10, int CustomerNumber = 0,
            string CompanyName = "", string CustomerName = "", string EmailAddress = "", int CompanyId = -1, int AccountTypeId = -1)
        {
            var customers = _customerService.GetPagedCustomers(page, pageSize, searchString,
                CustomerNumber,
                CompanyName,
                CustomerName,
                EmailAddress,
                CompanyId,
                AccountTypeId);

            ViewBag.SearchString = searchString;

            if (CustomerNumber > 0)
                ViewBag.CustomerNumber = CustomerNumber;
            else
                ViewBag.CustomerNumber = "";

            ViewBag.CompanyName = CompanyName;
            ViewBag.CustomerName = CustomerName;
            ViewBag.EmailAddress = EmailAddress;
            ViewBag.CompanyId = CompanyId;
            ViewBag.AccountTypeId = AccountTypeId;

            ViewBag.SavedSearchFiltersList = _savedSearchFilterService.GetAllSavedSearchFilterDto(EnumSavedSearchFilters.Customer, User.Identity.Name).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SavedSearchFilterName
            }).ToList();

            if (Request.IsAjaxRequest())
                return PartialView("_PagedCustomers", customers);

            return View(customers);
        }


        // GET: create
        [AuthorizeRoles("Administrator", "CanEditCustomer")]
        public ActionResult Create()
        {
            var model = new CustomerDto();
            model = LoadDefaultValues(model);

            return View(model);
        }


        [AuthorizeRoles("Administrator", "CanEditCustomer")]
        public ActionResult Edit(int id)
        {
            var mainCustomerModel = new MainCustomerDto();


            var customerModel = _customerService.GetCustomer(id);
            customerModel = LoadDefaultValues(customerModel);

            customerModel.SelectedCompanyId = customerModel.CompanyId.ToString();
            customerModel.SelectedAccountType = customerModel.AccountType.ToString();
            customerModel.SelectedAmountType = customerModel.AmountType.ToString();

            if (customerModel.CostPlusBasedWholeSalePriceType != null)
                customerModel.SelectedCostPlusBasedWholeSalePriceType = customerModel.CostPlusBasedWholeSalePriceType.ToString();

            ViewBag.Message = TempData["Message"];

            mainCustomerModel.customerModel = customerModel;
            mainCustomerModel.customerNotesModel = new CustomerNotesModel
            {
                customerNotesDto = new CustomerNotesDto()
                {
                    NotesCustomerId = id
                },
                customerNotesListDto = _customerService.GetCustomerNotesList(id)
            };

            mainCustomerModel.customerAddressModel = new CustomerAddressModel
            {
                customerAddressDto = new CustomerAddressDto()
                {
                    AddressCustomerId = id,
                    CountryList = _customerService.GetCountryList(),
                },
                customerAddressListDto = _customerService.GetCustomerAddressList(id)
            };

            mainCustomerModel.customerWholeSaleModel = new CustomerWholeSaleModel
            {
                customerScheduledTaskDto = new CustomerScheduledTaskDto(),
                customerScheduledTaskListDto = _customerScheduledTaskService.GetAllScheduledTasksByCustomerId(id)
            };

            return View(mainCustomerModel);
        }


        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanSaveCustomer")]
        public ActionResult Save(int id, CustomerDto model)
        {
            if (!ModelState.IsValid)
            {
                model = LoadDefaultValues(model);
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                ModelState.AddModelError("", string.Join("<br/>", errors));
                return View("create", model);
            }

            if (int.Parse(model.SelectedAccountType) == (int)CustomerAccountTypeEnum.Wholesale)
            {
                if (string.IsNullOrEmpty(model.SelectedCostPlusBasedWholeSalePriceType))
                {
                    model = LoadDefaultValues(model);

                    if (id <= 0)
                    {
                        ModelState.AddModelError("", "Select Cost Plus Based Wholesale Price Type.");
                        return View("create", model);
                    }
                    else
                    {
                        ModelState.AddModelError("", "Select Cost Plus Based Wholesale Price Type.");
                        return View("edit", model);
                    }
                }
            }

            if (_customerService.IsEmailExist(id, model.EmailAddress))
            {
                model = LoadDefaultValues(model);
                if (id <= 0)
                {
                    ModelState.AddModelError("", "Email address already exists");
                    return View("create", model);
                }
                else
                {
                    ModelState.AddModelError("", "Email address already exists");
                    return View("edit", model);
                }
            }

            // save the message template
            model.ModifiedBy = User.Identity.Name;
            if (id == 0)
                model = _customerService.CreateCustomer(model);
            else
                model = _customerService.UpdateCustomer(model);

            TempData["Message"] = "Changes have been successfully saved!";
            return RedirectToAction("edit", new { id = model.CustomerId });
        }


        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteCustomer")]
        public JsonResult _DeleteCustomer(int id)
        {
            try
            {
                _customerService.DeleteCustomer(id);

                return Json(new { Success = "Customer has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting Customer! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }

        private CustomerDto LoadDefaultValues(CustomerDto model)
        {
            var accountTypes = EnumHelper.GetEnumKeyValuePairList<CustomerAccountTypeEnum>();
            var accountTypeList = new SelectList(accountTypes, "Id", "Name", null);

            var CostPlusBasedWholeSalePriceTypes = EnumHelper.GetEnumKeyValuePairList<CostPlusBasedWholeSalePriceTypeEnum>();
            var CostPlusBasedWholeSalePriceTypeList = new SelectList(CostPlusBasedWholeSalePriceTypes, "Id", "Name", null);

            var AmountTypes = EnumHelper.GetEnumKeyValuePairList<AmountTypeEnum>();
            var AmountTypeList = new SelectList(AmountTypes, "Id", "Name", null);

            model.CompanyList = _companyService.GetAllCompanies().Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            }).ToList();
            model.AccountTypeList = accountTypeList.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value
            }).ToList();
            model.CostPlusBasedWholeSalePriceTypeList = CostPlusBasedWholeSalePriceTypeList.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value
            }).ToList();

            model.AmountTypeList = AmountTypeList.Select(x => new SelectListItem
            {
                Text = x.Text,
                Value = x.Value
            }).ToList();

            return model;
        }

        [AuthorizeRoles("Administrator", "CanViewAccountTypes")]
        public JsonResult _GetAllAccountTypes()
        {
            var accountTypes = EnumHelper.GetEnumKeyValuePairList<CustomerAccountTypeEnum>();
            var accountTypeList = new SelectList(accountTypes, "Id", "Name", null);

            var result = accountTypeList.Select(x => new
            {
                Name = x.Text,
                Id = x.Value
            }).ToList();

            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        [AuthorizeRoles("Administrator", "CanManageSearchFilter")]
        public JsonResult _ManageSearchFilter(int selectedSearchFilter, string filterName, string searchString)
        {
            var isNameExists = _savedSearchFilterService.IsFilterExist(selectedSearchFilter,
                EnumSavedSearchFilters.Customer,
                filterName, User.Identity.Name);
            if (isNameExists)
            {
                return Json(new { status = "error", message = "filter name already exists." }, JsonRequestBehavior.AllowGet);
            }


            var model = new SavedSearchFilterDto()
            {
                Created = DateTime.Now,
                CreatedBy = User.Identity.Name,
                Id = selectedSearchFilter,
                SavedSearchFilterId = Convert.ToInt32(EnumSavedSearchFilters.Customer),
                SavedSearchFilterName = filterName,
                SearchString = searchString
            };
            if (model.Id == 0)
                _savedSearchFilterService.CreateSavedSearchFilter(model);
            else
                _savedSearchFilterService.UpdateSavedSearchFilter(model);

            return Json(new { status = "success", message = "filter search saved successfully." }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanLoadSearchFilter")]
        public JsonResult _LoadSearchFilter()
        {
            var savedSearchFiltersList = _savedSearchFilterService.GetAllSavedSearchFilterDto(EnumSavedSearchFilters.Customer, User.Identity.Name).Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.SavedSearchFilterName
            }).ToList();

            return Json(new { status = "success", listItem = savedSearchFiltersList }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanGetSearchFilter")]
        public JsonResult _GetSearchFilterValues(int Id)
        {
            var savedSearchFiltersList = _savedSearchFilterService.GetSavedSearchFilter(Id);

            return Json(new { status = "success", item = savedSearchFiltersList }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanDeleteSearchFilter")]
        public JsonResult _DeleteSearchFilter(int Id)
        {
            try
            {
                _savedSearchFilterService.DeleteSavedSearchFilter(Id);

                return Json(new { status = "success", message = "Search filter has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { status = "error", message = "Error in deleting search filter! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }

        #region Customer's Images        
        [AuthorizeRoles("Administrator", "CanViewCustomerImages")]
        public JsonResult _GetCustomerFile(long id)
        {
            var productImage = _customerService.GetCustomerFile(id);

            return Json(productImage, JsonRequestBehavior.AllowGet);
        }

        public JsonResult _GetCustomerFiles(int customerId)
        {
            var images = _customerService.GetCustomerFiles(customerId);

            return Json(images, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditCustomerImages")]
        public ActionResult _SaveCustomerFile(int customerId, MediaContent model)
        {
            if (Request.Files.Count == 0)
                throw new ArgumentException("No image file attachment detected.");

            // parsed and save the image to a file
            var file = Request.Files[0];
            var filExtension = Path.GetExtension(file.FileName);
            using (var ms = new MemoryStream())
            {
                file.InputStream.CopyTo(ms);
                ms.Position = 0;
                model.Url = _fileHelper.SaveFile(_subFolderName, customerId.ToString(), filExtension, ms.ToArray());
            }

            model.Type = "CUSTOM";
            if (model.Id == -1)
                _customerService.AddCustomerFile(model);
            else
            {
                // delete the old image file
                var oldImage = _customerService.GetCustomerFile(model.Id);

                var fileName = Path.GetFileName(oldImage.Url);

                _fileHelper.RemoveFile(_subFolderName, oldImage.ParentId, fileName);

                _customerService.UpdateCustomerFile(model.Id, model.Url, model.Caption);
            }

            return Json(new { isUploaded = true, message = "Image file has been successfully uploaded" }, "text/html");
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditCustomerTaskFiles")]
        public ActionResult _SaveCustomerTaskFile(int Id, int customerId,string ImportFileName)
        {
            try
            {

                if (Id != -1 && !string.IsNullOrEmpty(ImportFileName))
                {
                    _fileHelper.RemoveFile(_subFolderName, customerId.ToString(), ImportFileName);
                }

                string fileUrl = "";
                // parsed and save the image to a file
                var file = Request.Files[0];
                var filExtension = Path.GetExtension(file.FileName);
                using (var ms = new MemoryStream())
                {
                    file.InputStream.CopyTo(ms);
                    ms.Position = 0;
                    fileUrl = _fileHelper.SaveFile(_subFolderName, customerId.ToString(), filExtension, ms.ToArray());
                }

                return Json(new { isUploaded = true, filePath = fileUrl, message = "Image file has been successfully uploaded" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { isUploaded = false, message = ex.ToString() }, JsonRequestBehavior.AllowGet);
            }
        }


        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteCustomerImages")]
        public JsonResult _DeleteCustomerFile(long id)
        {
            try
            {
                var productImage = _customerService.DeleteCustomerFile(id);

                return Json(productImage, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = string.Format("Unable to delete customer file! Err Message: {0}", EisHelper.GetExceptionMessage(ex)) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Notes

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanSaveCustomerNotes")]
        public ActionResult SaveNotes(CustomerNotesDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                ModelState.AddModelError("", string.Join("<br/>", errors));
                return View("Edit", model);
            }


            // save the message template
            model.ModifiedBy = User.Identity.Name;
            if (model.CustomerNotesId == -1)
                model = _customerService.CreateCustomerNotes(model);
            else
                model = _customerService.UpdateCustomerNotes(model);

            TempData["Message"] = "Changes have been successfully saved!";
            return RedirectToAction("edit", new { id = model.NotesCustomerId });
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteCustomerNotes")]
        public JsonResult _DeleteCustomerNotes(long id)
        {
            var customerNotes = _customerService.DeleteCustomerNotes(id);

            return Json(customerNotes, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanLoadCustomerNotes")]
        public JsonResult _GetCustomerNotes(int id)
        {

            var customerNotes = _customerService.GetCustomerNotes(id);

            return Json(customerNotes, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region Address

        [HttpPost, ValidateInput(false)]
        [AuthorizeRoles("Administrator", "CanSaveCustomerAddress")]
        public ActionResult SaveAddress(CustomerAddressDto model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(e => e.Errors.Select(x => x.ErrorMessage));
                ModelState.AddModelError("", string.Join("<br/>", errors));
                return View("Edit", model);
            }


            // save the message template
            model.ModifiedBy = User.Identity.Name;
            if (model.CustomerAddressID == -1)
                model = _customerService.CreateCustomerAddress(model);
            else
                model = _customerService.UpdateCustomerAddress(model);

            TempData["Message"] = "Changes have been successfully saved!";
            return RedirectToAction("edit", new { id = model.AddressCustomerId});
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteCustomerAddress")]
        public JsonResult _DeleteCustomerAddress(long id)
        {
            var customerAddress = _customerService.DeleteCustomerAddress(id);

            return Json(customerAddress, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanLoadCustomerAddress")]
        public JsonResult _GetCustomerAddress(int id)
        {

            var customerAddress = _customerService.GetCustomerAddress(id);

            return Json(customerAddress, JsonRequestBehavior.AllowGet);

        }

        #endregion

        #region CustomerWholesale

        #region ScheduledTasks
        [AuthorizeRoles("Administrator", "CanViewCustomerScheduledTasks")]
        public JsonResult _GetScheduledTasks()
        {
            var tasks = _customerScheduledTaskService.GetAllScheduledTasks();

            return Json(tasks, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewCustomerScheduledTasks")]
        public JsonResult _GetScheduledTask(int id)
        {
            var task = _customerScheduledTaskService.GetScheduledTask(id);

            return Json(task, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditCustomerScheduledTasks")]
        public JsonResult _SaveScheduledTask(string taskType)
        {
            var taskModel = new CustomerScheduledTaskDto();

            // update the model
            TryUpdateModel((dynamic)taskModel);
            taskModel.ModifiedBy = User.Identity.Name;

            if (taskModel.Id == -1)
                _customerScheduledTaskService.CreateScheduledTask(taskModel);
            else
                _customerScheduledTaskService.UpdateScheduledTask(taskModel.Id, taskModel);

            return Json(taskModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteCustomerScheduledTasks")]
        public JsonResult _DeleteScheduledTask(int id)
        {
            try
            {
                _customerScheduledTaskService.DeleteScheduledTask(id);

                return Json(new { Success = "Scheduled task has been successfully deleted!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting Scheduled task! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) });
            }
        }

        [AuthorizeRoles("Administrator", "CanViewGeneratedFilesFromCustomerTasks")]
        public JsonResult _GetTaskExportedFiles(int taskId, int page = 1)
        {
            var exportedFiles = _customerScheduledTaskService.GetPagedExportedFiles(taskId, page, 10);

            return Json(new
            {
                Items = exportedFiles,
                CurrentPageIndex = exportedFiles.PageNumber,
                EndItemIndex = exportedFiles.Count,
                StartItemIndex = 1, //exportedFiles.1,
                TotalItemCount = exportedFiles.TotalItemCount,
                TotalPageCount = exportedFiles.PageCount,
                ModelId = taskId
            }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanViewGeneratedFilesFromCustomerTasks")]
        public FileResult DownloadExportedFile(string fileName, string taskType)
        {
            var fileInfo = new FileInfo(string.Format("{0}\\{1}\\{2}",
                ConfigurationManager.AppSettings["ExportedFilesRoot"].ToString(),
                taskType,
                fileName));

            Response.Clear();
            Response.ContentType = getContentType(fileName);
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-disposition", string.Format("attachment; filename={0}", fileName));
            Response.TransmitFile(fileInfo.FullName);
            Response.Flush();
            Response.End();

            return null;
        }

        [AuthorizeRoles("Administrator", "CanExecuteScheduledCustomerTasks")]
        public JsonResult ExecuteScheduledTaskNow(int taskId)
        {
            _customerScheduledTaskService.SetTaskToExecuteNow(taskId);

            return Json(new { Success = "Scheduled task has been successfully set the flag to execute now!" },
                JsonRequestBehavior.AllowGet);
        }

        private string getContentType(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            switch (extension)
            {
                case ".csv":
                    return "text/csv";
                default:
                    return "text/plain";
            }
        }

        #endregion

        #region WholeSale Price Sku

        [AuthorizeRoles("Administrator", "CanViewCustomerWholeSalePriceHistory")]
        public JsonResult _GetCustomerWholeSalePriceHistory(string WholeSaleSearchSku)
        {
            var history = _customerService.GetCustomerWholeSalePriceHistorysList(WholeSaleSearchSku);

            return Json(history, JsonRequestBehavior.AllowGet);
        }

        #endregion

        #endregion

    }
}