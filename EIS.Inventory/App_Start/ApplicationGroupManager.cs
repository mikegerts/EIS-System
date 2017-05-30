using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using MySql.AspNet.Identity;

namespace EIS.Inventory
{
    public class ApplicationGroupManager : IDisposable
    {
        private readonly ApplicationGroupStore _groupStore;
        private readonly ApplicationUserManager _userManager;
        private readonly ApplicationRoleManager _roleManager;

        public ApplicationGroupManager()
        {
            _groupStore = new ApplicationGroupStore("InventoryConnection");
            _userManager = HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            _roleManager = HttpContext.Current.GetOwinContext().Get<ApplicationRoleManager>();
        }

        public IQueryable<ApplicationGroup> Groups
        {
            get { return _groupStore.Groups; }
        }

        public async Task<ApplicationGroup> FindByIdAsync(string id)
        {
            return await _groupStore.FindByIdAsync(id);
        }

        public async Task<ApplicationGroup> FindByNameAsync(string groupName)
        {
            return await _groupStore.FindByNameAsync(groupName);
        }

        public IQueryable<IdentityRole> GetGroupRoles(string groupId)
        {
            return _groupStore.GetGroupRoles(groupId);
        }

        public IQueryable<ApplicationGroup> GetUserGroups(string userId)
        {
            return _groupStore.GetUserGroups(userId);
        }

        public async Task<IdentityResult> CreateGroupAsync(ApplicationGroup group)
        {
            await _groupStore.CreateAsync(group);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateGroupAsync(ApplicationGroup group)
        {
            await _groupStore.UpdateAsync(group);

            // should update the selected roles for the group

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetGroupRolesAsync(string groupId, params string[] roleIds)
        {
            var thisGroup = await _groupStore.FindByIdAsync(groupId);

            // clear first the roles associated with this group id first
            await _groupStore.DeleteGroupRolesAsync(groupId);

            // add the new roles passed in
            var newRoles = _roleManager.Roles.Where(x => roleIds.Any(role => role == x.Id));
            if (newRoles.Any())
                await _groupStore.AddGroupRolesSync(groupId, newRoles.Select(x => x.Id).ToArray());

            // reset the roles for all affected users
            foreach (var userGroup in thisGroup.ApplicationUserGroups)
                await refreshUserGroupRolesAsync(userGroup.ApplicationUserId);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> SetUserGroupsAsync(string userId, params string[] groupIds)
        {
            // clear first the current group membership for this user
            await DeleteUserGroupsAync(userId);

            // add the new group for the user
            if (groupIds.Any())
                await _groupStore.AddUserGroupsAsync(userId, groupIds);

            // update the roles for the user
            await refreshUserGroupRolesAsync(userId);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteUserGroupsAync(string userId)
        {
            await _groupStore.DeleteUserGroupsAsync(userId);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteRoleGroupsAsync(string roleId)
        {
            await _groupStore.DeleteRoleGroupsAsync(roleId);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteGroupAsync(string groupId)
        {
            var group = await _groupStore.FindByIdAsync(groupId);
            if (group == null)
                return IdentityResult.Success;

            // delete first the assigned roles to the group
            await _groupStore.DeleteGroupRolesAsync(groupId);

            // then the users in the group
            await _groupStore.DeleteGroupUsersAsync(groupId);

            // lastly, the group itself
            await _groupStore.DeleteAsync(group);

            return IdentityResult.Success;
        }

        #region IDisposable
        public void Dispose()
        {
            _roleManager.Dispose();
            _userManager.Dispose();
            _groupStore.Dispose();
        }
        #endregion

        private async Task<IdentityResult> refreshUserGroupRolesAsync(string userId)
        {
            // delete the existing roles of the user
            var oldUserRoles = await _userManager.GetRolesAsync(userId);
            if (oldUserRoles.Any())
                await _userManager.RemoveFromRolesAsync(userId, oldUserRoles.ToArray());

            // find the roles this use is entitled to from group membership
            var newGroupRoles = _groupStore.GetUserGroupRoles(userId);

            // get the damn role names
            var allRoles = _roleManager.Roles.ToList();
            var rolesToAdd = allRoles.Where(x => newGroupRoles.Any(role => role.RoleId == x.Id));
            var roleNames = rolesToAdd.Select(x => x.Name).ToArray();

            // add the new roles for the user
            await _userManager.AddToRolesAsync(userId, roleNames);

            return IdentityResult.Success;
        }
    }
}