using System;
using System.Configuration;
using System.IO;
using System.Web.Mvc;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using System.Security.Claims;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ISystemJobService _jobService;

        public HomeController(ISystemJobService jobService)
        {
            _jobService = jobService;
        }

        public ActionResult Index()
        { 
            var identity = (ClaimsIdentity)User.Identity;

            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }

        [HttpPost]
        public JsonResult _CheckFtpConnection(Credential credential)
        {
            var result = FtpWebRequestor.CheckFtpConnection(credential);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult _CheckFileFromFtp(Credential credential)
        {
            var result = FtpWebRequestor.CheckFileFromFtp(credential);

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetFileTemplate(string fileTemplateName)
        {
            // get the 
            var fileInfo = new FileInfo(string.Format("{0}{1}", Server.MapPath("~/TemplateFiles/"), fileTemplateName));

            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-disposition", string.Format("attachment; filename=\"{0}\"", fileTemplateName));
            Response.TransmitFile(fileInfo.FullName);
            Response.Flush();
            Response.End();

            return null;
        }

        public ActionResult DownloadSystemJobFileResult(int jobId)
        {
            var resultFilePath = _jobService.GetSystemJobParameterOut(jobId);
            if (string.IsNullOrEmpty(resultFilePath))
                return null;

            var fileInfo = new FileInfo(resultFilePath);
            Response.Clear();
            Response.ContentType = "text/csv";
            Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Response.AddHeader("Content-disposition", string.Format("attachment; filename=\"{0}\"", fileInfo.Name));
            Response.TransmitFile(fileInfo.FullName);
            Response.Flush();
            Response.End();

            return null;
        }
        
        /// <summary>
        /// Handles file upload
        /// </summary>
        /// <param name="isCreate"></param>
        /// <param name="hasHeaderFile"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult UploadFileAsync(JobType jobType, bool isCreate = false, bool isAutoLink = false, bool isCreateEisSKU = false, bool hasHeaderFile = true,string supportiveParameters = null)
        {
            var file = Request.Files[0];
            var fileFullPath = string.Format("{0}\\{2:yyyy}{2:MM}{2:dd}{2:HH}{2:mm}_{1}",
                ConfigurationManager.AppSettings["FilePath"].ToString(), file.FileName, DateTime.UtcNow);

            // save the file to the file path
            file.SaveAs(fileFullPath);

            // creat jobs to process the uploading product file
            var systemJob = new SystemJobDto
            {
                JobType = jobType,
                Parameters = fileFullPath,
                IsAddNewItem = isCreate,
                HasPostAction_1 = isAutoLink, // this flag is for add link to EIS products with vendor products via UPC code
                HasPostAction_2 = isCreateEisSKU, // this flag for adding link to EIS products and create EIS product it doesn't exist
                HasHeader = hasHeaderFile,
                SubmittedBy = User.Identity.Name,
                SupportiveParameters = supportiveParameters
            };
            _jobService.CreateSystemJob(systemJob);

            return Json(new { isUploaded = true, message = "The file has been successfuly uploaded and passed to EIS System Job service for execution." }, "text/html");
        }

        protected override void Dispose(bool disposing)
        {
            _jobService.Dispose();
            base.Dispose(disposing);
        }
    }
}