using System.Linq;
using System.Web.Mvc;
using Webdiyer.WebControls.Mvc;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.Services;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class LogsController : Controller
    {
        private readonly IReportLogService _service;

        public LogsController(IReportLogService service)
        {
            _service = service;
        }

        // GET: Logs
        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public ActionResult Index(LogsType logsType, int page = 1, int pageSize = 10)
        {
            if (logsType == LogsType.RequestReport)
                return RedirectToAction("RequestReportList", new { pageSize = pageSize });
            else if (logsType == LogsType.FileUploader)
                return RedirectToAction("FileUploaderList", new { pageSize = pageSize });
            else
                return RedirectToAction("LogList", new { pageSize = pageSize });
        }

        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public ActionResult RequestReportList(int page = 1, int pageSize = 10)
        {
            var requestReports = _service.GetAllRequestReports()
                   .OrderByDescending(x => x.Id)
                   .ToPagedList(page, pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_LoadPagedRequestReports", requestReports);

            return View(requestReports);
        }

        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public ActionResult LogList(int page = 1, int pageSize = 10)
        {
            var logs = _service.GetAllLogs()
                .OrderByDescending(x => x.Id)
                   .ToPagedList(page, pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_LoadPagedMainLogs", logs);

            return View(logs);
        }

        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public ActionResult FileUploaderList(int page = 1, int pageSize = 10)
        {
            var logs = _service.GetFileUploaderLogs()
                .OrderByDescending(x => x.Id)
                   .ToPagedList(page, pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_LoadPagedFileUploader", logs);

            return View(logs);
        }

        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public JsonResult _GetFileUploaderLog(int id)
        {
            var log = _service.GetFileUploaderLog(id);

            return Json(log, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public JsonResult _GetLog(int id)
        {
            var log = _service.GetLog(id);

            return Json(log, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public JsonResult _GetRequestProcessingReport(string requestReportId)
        {
            var processingReport = _service.GetProcessingReport(requestReportId);

            return Json(processingReport, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public JsonResult _GetProcessingReportErrors(string requestReportId, int page = 1)
        {
            var errResults = _service.GetProcessingReportErrors(requestReportId)
                .OrderBy(x => x.MessageId)
                .ToPagedList(page, 10);

            return Json(new
            {
                Items = errResults,
                CurrentPageIndex = errResults.CurrentPageIndex,
                EndItemIndex = errResults.EndItemIndex,
                StartItemIndex = errResults.StartItemIndex,
                TotalItemCount = errResults.TotalItemCount,
                TotalPageCount = errResults.TotalPageCount,
                TransactionId = requestReportId
            }, JsonRequestBehavior.AllowGet);
        }

        [AuthorizeRoles("Administrator", "CanViewLogs")]
        public JsonResult _GetProcessingReportWarnings(string requestReportId, int page = 1)
        {
            var warningResults = _service.GetProcessingReportWarnings(requestReportId)
                .OrderBy(x => x.MessageId)
                .ToPagedList(page, 10);

            return Json(new
            {
                Items = warningResults,
                CurrentPageIndex = warningResults.CurrentPageIndex,
                EndItemIndex = warningResults.EndItemIndex,
                StartItemIndex = warningResults.StartItemIndex,
                TotalItemCount = warningResults.TotalItemCount,
                TotalPageCount = warningResults.TotalPageCount,
                TransactionId = requestReportId
            }, JsonRequestBehavior.AllowGet);
        }
    }
}