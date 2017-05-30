using System;
using System.Web.Mvc;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Shared.ViewModels;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class CredentialController : Controller
    {
        private readonly ICredentialService _credentialService;

        public CredentialController(ICredentialService credentialService)
        {
            _credentialService = credentialService;
        }

        // GET: Credenital
        [AuthorizeRoles("Administrator", "CanViewCredentials")]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanViewCredentials")]
        public JsonResult _GetCredentials()
        {
            var credentials = _credentialService.GetCredentials();

            return Json(credentials, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator", "CanEditCredentials")]
        public JsonResult _GetCredential(int id)
        {
            var credential = _credentialService.GetCredential(id);

            return Json(credential, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanEditCredentials")]
        public JsonResult _SaveCredential(string marketplaceType)
        {
            var model = createCredentialModel(marketplaceType);

            // update the model
            TryUpdateModel((dynamic)model);
            model.ModifiedBy = User.Identity.Name;

            if (model.Id == -1)
                _credentialService.CreateCredential(model);
            else
                _credentialService.UpdateCredential(model.Id, model);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator", "CanDeleteCredentials")]
        public JsonResult _DeleteCredential(int id)
        {
            try
            {
                _credentialService.DeleteCredential(id);

                return Json(new { Success = "Credential has been successfully deleted!" });
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting credential! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) });
            }
        }

        private CredentialDto createCredentialModel(string credentialType)
        {
            CredentialDto model = null;

            if (credentialType == CredentialType.EBAY)
                model = new eBayCredentialDto();
            else if (credentialType == CredentialType.AMAZON)
                model = new AmazonCredentialDto();
            else if (credentialType == CredentialType.SHIP_STATION)
                model = new ShipStationCredentialDto();
            else if (credentialType == CredentialType.BIG_COMMERCE)
                model = new BigCommerceCredentialDto();

            return model;
        }
    }
}