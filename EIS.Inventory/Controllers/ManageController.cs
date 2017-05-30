using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using X.PagedList;
using EIS.Inventory.Core.Services;
using EIS.Inventory.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using MySql.AspNet.Identity;
using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private ApplicationGroupManager _groupManager;
        private ApplicationRoleManager _roleManager;
        private readonly IVendorService _vendorService;

        public ManageController(IVendorService vendorService)
        {
            _vendorService = vendorService;
        }

        public ApplicationSignInManager SignInManager
        {
            get { return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>(); }
            private set { _signInManager = value; }
        }

        public ApplicationUserManager UserManager
        {
            get { return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>(); }
            private set { _userManager = value; }
        }
        
        public ApplicationRoleManager RoleManager
        {
            get { return _roleManager ?? HttpContext.GetOwinContext().Get<ApplicationRoleManager>(); }
            private set { _roleManager = value; }
        }

        public ApplicationGroupManager GroupManager
        {
            get { return _groupManager ?? new ApplicationGroupManager(); }
            private set { _groupManager = value; }
        }

        #region user's groups
        [AuthorizeRoles("Administrator")]
        public ActionResult Groups(string searchString, int page = 1, int pageSize = 10)
        {
            // get the list of groups
            var groups = GroupManager.Groups
                .Where(x => string.IsNullOrEmpty(searchString) || x.Name.Contains(searchString))
                .OrderBy(x => x.Name)
                .Select(group => new GroupViewModel
                {
                    Id = group.Id,
                    Name = group.Name,
                    Description = group.Description,
                    NumberOfUsers = group.NumberOfUsers
                })
                .ToPagedList(page, pageSize);

            ViewBag.SearchString = searchString;

            if (Request.IsAjaxRequest())
                return PartialView("_PagedGroups", groups);

            return View(groups);
        }

        [AuthorizeRoles("Administrator")]
        public ActionResult CreateGroup()
        {
            var group = new GroupViewModel();
            populateRolesForGroup(group);

            return View(group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> CreateGroup(GroupViewModel model)
        {
            if (ModelState.IsValid)
            {
                // determine if the group's name already exists
                var applicationGroup = await GroupManager.FindByNameAsync(model.Name);
                if (applicationGroup != null)
                {
                    populateRolesForGroup(model);
                    ModelState.AddModelError("", string.Format("Group name \'{0}\' already exist!", model.Name));

                    return View(model);
                }

                // save the group to the database
                var group = new ApplicationGroup { Name = model.Name, Description = model.Description };
                var result = await GroupManager.CreateGroupAsync(group);
                if (result.Succeeded)
                {
                    // add the selected permissions for the group
                    await GroupManager.SetGroupRolesAsync(group.Id, model.SelectedRoles.ToArray());

                    // if we got this far, everything is OK
                    TempData["Message"] = "Changes have been successfully saved!";

                    // rederict to the edit page
                    return RedirectToAction("editgroup", new { id = group.Id });
                }

                // add errors
                addErrors(result);
            }

            // repopulate the 
            populateRolesForGroup(model);

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Please verify the required fields are filled or you entered correct data.");
            return View(model);
        }

        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> EditGroup(string id)
        {
            var group = await GroupManager.FindByIdAsync(id);
            if (group == null)
                return HttpNotFound(string.Format("The group with id {0} was not found!", id));

            var model = new GroupViewModel()
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description
            };

            // populate the group roles
            populateRolesForGroup(model, group.ApplicationGroupRoles);
            ViewBag.Message = TempData["Message"];

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> EditGroup(GroupViewModel model)
        {
            // get first the group
            var group = await GroupManager.FindByIdAsync(model.Id);
            if (group == null)
                return HttpNotFound(string.Format("The group with id {0} was not found!", model.Id));

            if (ModelState.IsValid)
            {
                // let's update the group information first
                group.Name = model.Name;
                group.Description = model.Description;
                await GroupManager.UpdateGroupAsync(group);

                // then, set the new selected roles for the groups
                await GroupManager.SetGroupRolesAsync(model.Id, model.SelectedRoles.ToArray());

                // if we got this far, everything is OK
                TempData["Message"] = "Changes have been successfully saved!";

                // rederict to the edit page
                return RedirectToAction("editgroup", new { id = model.Id });
            }

            // repopulate the 
            populateRolesForGroup(model, group.ApplicationGroupRoles);

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Please verify the required fields are filled or you entered correct data.");
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        public async Task<JsonResult> _DeleteGroup(string id)
        {
            try
            {
                var isSuccess = await GroupManager.DeleteGroupAsync(id);

                return Json(new { Success = "Group has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting Group! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region user's roles
        // GET: RolesAdmin
        [AuthorizeRoles("Administrator")]
        public ActionResult Roles(string searchString, int page = 1, int pageSize = 10)
        {
            var roles = RoleManager.Roles
                .Where(x => string.IsNullOrEmpty(searchString) || x.Name.Contains(searchString))
                .OrderBy(x => x.Order)
                .Select(role => new RoleViewModel
                {
                    Id = role.Id,
                    RoleName = role.Name,
                    Description = role.Description,
                    Order = role.Order
                })
                .ToPagedList(page, pageSize);

            ViewBag.SearchString = searchString;

            if (Request.IsAjaxRequest())
                return PartialView("_PagedRoles", roles);

            return View(roles);
        }

        [HttpGet]
        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> CreateRole()
        {
            var maxOrder = await RoleManager.GetMaxOrder();

            return View(new RoleViewModel { Order = maxOrder + 10 });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> CreateRole(RoleViewModel model)
        {
            var role = new IdentityRole { Name = model.RoleName, Description = model.Description, Order = model.Order };

            // determine if the role's name already exists
            var roleIdentity = await RoleManager.FindByNameAsync(role.Name);
            if (roleIdentity != null)
            {
                ModelState.AddModelError("", string.Format("Role name \'{0}\' already exist!", model.RoleName));
                return View();
            }
            else
            {
                RoleManager.Create(role);

                // if we got this far, everything is OK
                TempData["Message"] = "Changes have been successfully saved!";

                return RedirectToAction("editrole", new { id = role.Id });
            }
        }

        [HttpGet]
        [Authorize]
        [AuthorizeRoles("Administrator")]
        public ActionResult EditRole(string id)
        {
            var role = RoleManager.FindById(id);
            var model = new RoleViewModel() { Id = role.Id, RoleName = role.Name, Description = role.Description, Order = role.Order };
            ViewBag.Message = TempData["Message"];

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> EditRole(RoleViewModel model)
        {
            // get first the updated role data
            var updatedRole = RoleManager.FindById(model.Id);

            // let's us check if the new role name does exist
            var newRoleName = await RoleManager.FindByNameAsync(model.RoleName);
            if (newRoleName != null && !string.Equals(updatedRole.Name, model.RoleName, StringComparison.InvariantCultureIgnoreCase))
            {
                ModelState.AddModelError("", string.Format("Role name \'{0}\' already exist!", model.RoleName));
                return View(model);
            }

            // otherwise, update the role name
            updatedRole.Name = model.RoleName;
            updatedRole.Description = model.Description;
            updatedRole.Order = model.Order;
            RoleManager.Update(updatedRole);

            // if we got this far, everything is OK
            TempData["Message"] = "Changes have been successfully saved!";

            return RedirectToAction("editrole", new { id = model.Id });
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        public async Task<JsonResult> _DeleteRole(string id)
        {
            try
            {
                var role = await RoleManager.FindByIdAsync(id);
                if (role == null)
                    return Json(new { Success = string.Format("Role with id: \'{0}\' was not found!", id) }, JsonRequestBehavior.AllowGet);

                if (role.Name == "Administrator")
                    return Json(new { Success = "Administrator role cannot be deleted!" }, JsonRequestBehavior.AllowGet);

                // delete first the role's groups
                await GroupManager.DeleteRoleGroupsAsync(id);

                RoleManager.Delete(role);

                return Json(new { Success = "Role has been successfully deleted!" },
                JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting Role! <br/>Exception: " + EisHelper.GetExceptionMessage(ex) },
                JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region users
        [AuthorizeRoles("Administrator")]
        public ActionResult Users(string searchString, int page = 1, int pageSize = 10)
        {
            var users = UserManager.Users
                .Where(x => string.IsNullOrEmpty(searchString) || x.FirstName.Contains(searchString))
                .Select(user => new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    LastLoginDate = user.LastLoginDate,
                    Group = GroupManager.GetUserGroups(user.Id).Select(x => x.Name).FirstOrDefault()
                })
                .OrderBy(x => x.FirstName)
                .ThenBy(x => x.LastName)
                .ToPagedList(page, pageSize);

            ViewBag.SearchString = searchString;

            if (Request.IsAjaxRequest())
                return PartialView("_PagedUsers", users);

            return View(users);
        }

        [AuthorizeRoles("Administrator")]
        public ActionResult CreateUser()
        {
            var model = new UserViewModel();

            // populate user's groups view page
            populeGroupsViewBag(model.Group);

            // populate user's assigned vendors
            populateUserVendors(model);

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> CreateUser(UserViewModel model)
        {

            // populate user's groups view page
            populeGroupsViewBag(model.Group);

            // populate user's assigned vendors
            populateUserVendors(model);

            if (ModelState.IsValid)
            {
                // let's determine first if the new user's username already exists
                var existingUser = await UserManager.FindByNameAsync(model.UserName);
                if (existingUser != null)
                {
                    ModelState.AddModelError("", string.Format("User with username \'{0}\' already exist!", model.UserName));
                    return View(model);
                }

                // create the user domain to save
                var user = new ApplicationUser
                {
                    UserName = model.UserName,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Website = model.Website,
                    AddressLine1 = model.AddressLine1,
                    AddressLine2 = model.AddressLine2,
                    State = model.State,
                    ZipCode = model.ZipCode
                };

                var result = await UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // add user to the selected groups
                    await GroupManager.SetUserGroupsAsync(user.Id, new string[] { model.Group });

                    // add the user selected vendors
                    foreach (var item in model.SelectedVendors)
                        await UserManager.AddClaimAsync(user.Id, new Claim(ClaimType.VENDOR, item));

                    // if we got this far, everything is OK
                    TempData["Message"] = "Changes have been successfully saved!";

                    return RedirectToAction("edituser", new { id = user.Id });
                }

                // add the errors
                addErrors(result);
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Please verify the required fields are filled or you entered correct data.");
            return View(model);
        }

        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> EditUser(string id)
        {
            if (string.IsNullOrEmpty(id))
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

            // get the user
            var user = await UserManager.FindByIdAsync(id);
            if (user == null)
                return HttpNotFound(string.Format("User with id: \'{0}\' was not found!", id));

            // parse the user into view model
            var model = new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Website = user.Website,
                AddressLine1 = user.AddressLine1,
                AddressLine2 = user.AddressLine2,
                State = user.State,
                ZipCode = user.ZipCode,
                LastLoginDate = user.LastLoginDate.HasValue ? user.LastLoginDate.Value : DateTime.MinValue,
                Group = GroupManager.GetUserGroups(user.Id).Select(x => x.Id).FirstOrDefault()
            };

            // populate user's groups view page
            populeGroupsViewBag(model.Group);
            populateUserVendors(model);
            ViewBag.Message = TempData["Message"];

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRoles("Administrator")]
        public async Task<ActionResult> EditUser(UserViewModel model)
        {
            // populate user's groups view page
            populeGroupsViewBag(model.Group);

            // populate user's assigned vendors
            populateUserVendors(model);

            if (ModelState.IsValid)
            {
                // get the user's first by username, we disabled editing the user's username
                var user = await UserManager.FindByNameAsync(model.UserName);
                if (user == null)
                    return HttpNotFound(string.Format("User with username: \'{0}\' was not found!", model.UserName));

                // update the user changes
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.Email = model.Email;
                user.PhoneNumber = model.PhoneNumber;
                user.Website = model.Website;
                user.AddressLine1 = model.AddressLine1;
                user.AddressLine2 = model.AddressLine2;
                user.State = model.State;
                user.ZipCode = model.ZipCode;
                await UserManager.UpdateAsync(user);

                // updat the user's groups membership
                await GroupManager.SetUserGroupsAsync(model.Id, new string[] { model.Group });

                // get all the current claims for the user
                var currentUserClaims = await UserManager.GetClaimsAsync(user.Id);
                var vendorClaims = currentUserClaims.Where(x => x.Type == ClaimType.VENDOR);

                // remove all the claims which it is from Vendor
                foreach (var claim in vendorClaims)
                    await UserManager.RemoveClaimAsync(user.Id, claim);

                // add the user selected vendors
                foreach (var item in model.SelectedVendors)
                    await UserManager.AddClaimAsync(user.Id, new Claim(ClaimType.VENDOR, item));

                // if we got this far, everything is OK
                TempData["Message"] = "Changes have been successfully saved!";

                return RedirectToAction("edituser", new { id = model.Id });
            }

            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Please verify the required fields are filled or you entered correct data.");
            return View(model);
        }

        [HttpPost]
        [AuthorizeRoles("Administrator")]
        public async Task<JsonResult> _DeleteUser(string id)
        {
            try
            {
                var user = UserManager.FindById(id);
                if (user == null)
                    return Json(new { Success = string.Format("User with id: \'{0}\' was not found!", id) }, JsonRequestBehavior.AllowGet);

                // remove the user from the groups
                await GroupManager.DeleteUserGroupsAync(user.Id);

                UserManager.Delete(user);

                return Json(new { Success = "User has been successfully deleted!" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { Error = "Error in deleting user! - Message: " + EisHelper.GetExceptionMessage(ex) }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region manage predefined actions
        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, false, false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, false, false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, false, false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, false, false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, false, false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, false, false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            addErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, false, false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                addErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }
        #endregion

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _vendorService.Dispose();
            }

            base.Dispose(disposing);
        }

        #region Helpers
        private void populateRolesForGroup(GroupViewModel model, List<ApplicationGroupRole> groupRoles = null)
        {
            if (model.RolesList == null)
                model.RolesList = new List<RoleViewModel>();

            // get the list of available roles or permissions
            var roles = RoleManager.Roles;
            roles.ToList().ForEach(r => model.RolesList.Add(new RoleViewModel
            {
                Id = r.Id,
                RoleName = r.Name,
                Description = r.Description,
                IsSelected = (groupRoles == null) ? false : groupRoles.Exists(g => g.RoleId == r.Id),
            }));
        }

        private void populeGroupsViewBag(object selectedGroup = null)
        {
            var allGroups = GroupManager.Groups.ToList();

            ViewBag.GroupsList = new SelectList(allGroups, "Id", "Name", selectedGroup);
        }

        private void populateUserVendors(UserViewModel model)
        {
            // get the assigned vendors for the user via claims
            var userClaims = UserManager.GetClaims(model.Id);
            var allVendors = _vendorService.GetAllVendors();

            var vendorList = new List<ItemViewModel>();
            foreach (var vendor in allVendors)
            {
                vendorList.Add(new ItemViewModel
                {
                    Id = vendor.Id,
                    Name = vendor.Name,
                    IsSelected = userClaims.Any(x => x.Type == ClaimType.VENDOR 
                        && x.Value == vendor.Id.ToString())
                });
            }

            model.VendorList = vendorList;
        }

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void addErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

#endregion
    }
}