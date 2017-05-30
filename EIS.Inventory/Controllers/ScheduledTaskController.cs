using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class ScheduledTaskController : Controller
    {
        private readonly IScheduledTaskService _service;

        public ScheduledTaskController(IScheduledTaskService service)
        {
            _service = service;
        }

        // GET: ScheduledTask
        [AuthorizeRoles("Administrator", "CanViewScheduledTasks")]
        public ActionResult Index()
        {
            return View();
        }

        [AuthorizeRoles("Administrator", "CanViewScheduledTasks")]
        public JsonResult _GetScheduledTasks()
        {
            var tasks = _service.GetAllScheduledTasks();

            return Json(tasks, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewScheduledTasks")]
        public JsonResult _GetScheduledTask(int id)
        {
            var task = _service.GetScheduledTask(id);

            return Json(task, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditScheduledTasks")]
        public JsonResult _SaveScheduledTask(string taskType)
        {
            var taskModel = createScheduletedTask(taskType);
            
            // update the model
            TryUpdateModel((dynamic)taskModel);
            taskModel.ModifiedBy = User.Identity.Name;

            if (taskModel.Id == -1)
                _service.CreateScheduledTask(taskModel);
            else
                _service.UpdateScheduledTask(taskModel.Id, taskModel);

            return Json(taskModel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteScheduledTasks")]
        public JsonResult _DeleteScheduledTask(int id)
        {
            try
            {
                _service.DeleteScheduledTask(id);

                return Json(new { Success = "Scheduled task has been successfully deleted!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting Scheduled task! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) });
            }
        }

        [AuthorizeRoles("Administrator", "CanViewGeneratedFilesFromTasks")]
        public JsonResult _GetTaskExportedFiles(int taskId, int page = 1)
        {
            var exportedFiles = _service.GetPagedExportedFiles(taskId, page, 10);

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
        [AuthorizeRoles("Administrator", "CanViewGeneratedFilesFromTasks")]
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

        [AuthorizeRoles("Administrator", "CanExecuteScheduledTasks")]
        public JsonResult ExecuteScheduledTaskNow(int taskId)
        {
            _service.SetTaskToExecuteNow(taskId);

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

        private ScheduledTaskDto createScheduletedTask(string taskType)
        {
            ScheduledTaskDto task = null;
            if (taskType == ScheduledTaskType.CUSTOM_EXPORT_ORDER)
                task = new CustomExportOrderTaskDto();
            else if (taskType == ScheduledTaskType.GENERATE_PO)
                task = new GeneratePoTaskDto();
            else if (taskType == ScheduledTaskType.MARKETPLACE_INVENTORY)
                task = new MarketplaceInventoryTaskDto();
            else if (taskType == ScheduledTaskType.VENDOR_PRODUCT_FILE_INVENTORY)
                task = new VendorProductFileInventoryTaskDto();
            else if (taskType == ScheduledTaskType.CUSTOM_EXPORT_PRODUCT)
                task = new CustomExportProductTaskDto();
            else if (taskType == ScheduledTaskType.CUSTOM_IMPORT_ORDER)
                task = new CustomImportOrderTaskDto();
            else
                throw new ArgumentException("Unknown task type: " + task.TaskType);

            return task;
        }
    }
}