using System;
using System.Linq;
using System.Threading.Tasks;
using MySql.AspNet.Identity.Repositories;
using System.Configuration;
using Microsoft.AspNet.Identity;

namespace MySql.AspNet.Identity
{
    public class ApplicationGroupStore : IDisposable
    {
        private bool _isDisposed;
        private string _connectionString;
        private GroupRepository _groupRepository;
        private UserRoleGroupRepository _userRoleGroupRepository;

        public ApplicationGroupStore()
            : this("DefaultConnection")
        {
        }

        public ApplicationGroupStore(string connectionStringName)
        {
            _connectionString = ConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;

            _groupRepository = new GroupRepository(_connectionString);
            _userRoleGroupRepository = new UserRoleGroupRepository(_connectionString);
        }

        public IQueryable<ApplicationGroup> Groups
        {
            get { return _groupRepository.GetGroups(); }
        }

        public Task<ApplicationGroup> FindByIdAsync(string groupId)
        {
            var result = _groupRepository.GetGroupById(groupId);

            result.ApplicationUserGroups = _userRoleGroupRepository.PopulateUserGroups(groupId);
            result.ApplicationGroupRoles = _userRoleGroupRepository.PopulateGroupRoles(groupId);

            return Task.FromResult(result);
        }

        public Task<ApplicationGroup> FindByNameAsync(string groupName)
        {
            var result = _groupRepository.GetGroupByName(groupName);

            return Task.FromResult(result);
        }

        public IQueryable<ApplicationGroup> GetUserGroups(string userId)
        {
            return _userRoleGroupRepository.GetUserGroups(userId);
        }

        public IQueryable<IdentityRole> GetGroupRoles(string groupId)
        {
            return _userRoleGroupRepository.GetGroupRoles(groupId);
        }

        public IQueryable<ApplicationGroupRole> GetUserGroupRoles(string userId)
        {
            return _userRoleGroupRepository.GetUserGroupRoles(userId);
        }

        public Task CreateAsync(ApplicationGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            _groupRepository.Insert(group);

            return Task.FromResult<object>(null);
        }

        public Task DeleteAsync(ApplicationGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            _groupRepository.Delete(group.Id);

            return Task.FromResult<Object>(null);
        }

        public Task UpdateAsync(ApplicationGroup group)
        {
            if (group == null)
            {
                throw new ArgumentNullException("group");
            }

            _groupRepository.Update(group);

            return Task.FromResult<Object>(null);
        }

        public Task AddGroupRolesSync(string groupId, params string[] roleIds)
        {
            _userRoleGroupRepository.AddGroupRoles(groupId, roleIds);

            return Task.FromResult<Object>(true);
        }

        public Task AddUserGroupsAsync(string userId, params string[] groupIds)
        {
            _userRoleGroupRepository.AddUserGroups(userId, groupIds);

            return Task.FromResult<Object>(true);
        }

        public Task DeleteGroupRolesAsync(string groupId)
        {
            _userRoleGroupRepository.DeleteGroupRoles(groupId);

            return Task.FromResult<Object>(true);
        }

        public Task DeleteRoleGroupsAsync(string roleId)
        {
            _userRoleGroupRepository.DeleteRoleGroups(roleId);

            return Task.FromResult<Object>(true);
        }

        public Task DeleteGroupUsersAsync(string groupId)
        {
            _userRoleGroupRepository.DeleteGroupUsers(groupId);

            return Task.FromResult<Object>(true);
        }

        public Task DeleteUserGroupsAsync(string userId)
        {
            _userRoleGroupRepository.DeleteUserGroups(userId);

            return Task.FromResult<Object>(true);
        }

        public void Dispose()
        {
            if (_isDisposed)
            {
                _groupRepository = null;
                _userRoleGroupRepository = null;
            }
        }
    }
}
