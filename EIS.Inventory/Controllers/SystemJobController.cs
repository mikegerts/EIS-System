using System.Web.Mvc;
using EIS.Inventory.Core.Services;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class SystemJobController : Controller
    {
        private readonly ISystemJobService _service;

        public SystemJobController(ISystemJobService service)
        {
            _service = service;
        }

        // GET: System
        [AuthorizeRoles("Administrator", "CanViewSystemJobs")]
        public ActionResult Index(int page = 1, int pageSize = 10)
        {
            var jobs = _service.GetPagedSystemJobs(page, pageSize);

            if (Request.IsAjaxRequest())
                return PartialView("_PagedSystemJobs", jobs);

            return View(jobs);
        }
    }
}