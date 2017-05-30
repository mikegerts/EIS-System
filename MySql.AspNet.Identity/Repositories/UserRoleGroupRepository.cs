using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using MySql.Data.MySqlClient;
using System;

namespace MySql.AspNet.Identity.Repositories
{
    public class UserRoleGroupRepository
    {
        private readonly string _connectionString;
        public UserRoleGroupRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Get the list of application groups assigned to the user
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns></returns>
        public IQueryable<ApplicationGroup> GetUserGroups(string userId)
        {
            var groups = new List<ApplicationGroup>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@ApplicationUserId", userId}
                };
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT g.Id, g.Name, g.Description FROM applicationgroups g
                    INNER JOIN applicationusergroups ug
	                    ON ug.ApplicationGroupId = g.Id
                        AND ug.ApplicationUserId = @ApplicationUserId
                     ORDER BY g.Name",
                    parameters);

                while (reader.Read())
                {
                    var group = new ApplicationGroup();
                    group.Id = reader[0].ToString();
                    group.Name = reader[1].ToString();
                    group.Description = reader[2].ToString();

                    groups.Add(group);
                }
            }
            return groups.AsQueryable();
        }

        /// <summary>
        /// Get the list of roles assigned to the specified group id
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <returns></returns>
        public IQueryable<IdentityRole> GetGroupRoles(string groupId)
        {
            var roles = new List<IdentityRole>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@groupId", groupId}
                };
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT r.Id, r.Name, r.Description, r.Order FROM aspnetroles r
                    INNER JOIN applicationgrouproles gr
	                    ON gr.RoleId = r.Id AND ApplicationGroupId = @groupId
                    ORDER BY r.Order;",
                    parameters);

                while (reader.Read())
                {
                    var role = new IdentityRole();
                    role.Id = reader[0].ToString();
                    role.Name = reader[1].ToString();
                    role.Description = reader[2].ToString();
                    role.Order = Convert.ToInt32(reader[3]);
                    roles.Add(role);
                }
            }
            return roles.AsQueryable();
        }

        /// <summary>
        /// Get the list of application group roles for the user
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <returns></returns>
        public IQueryable<ApplicationGroupRole> GetUserGroupRoles(string userId)
        {
            var groupRoles = new List<ApplicationGroupRole>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@ApplicationUserId", userId}
                };
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT r.Id, gr.ApplicationGroupId FROM aspnetroles r 
                    INNER JOIN applicationgrouproles gr
	                    ON gr.RoleId = r.Id
                    INNER JOIN applicationusergroups ug
	                    ON ug.ApplicationGroupId = gr.ApplicationGroupId
                        AND ApplicationUserId = @ApplicationUserId;",
                    parameters);

                while (reader.Read())
                {
                    var groupRole = new ApplicationGroupRole();
                    groupRole.RoleId = reader[0].ToString();
                    groupRole.ApplicationGroupId = reader[1].ToString();

                    groupRoles.Add(groupRole);
                }
            }
            return groupRoles.AsQueryable();
        }

        /// <summary>
        /// Add the user to the list of groups
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="groupIds">The array of groups which the user will be added</param>
        public void AddUserGroups(string userId, params string[] groupIds)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var sCommand = new StringBuilder("INSERT INTO applicationusergroups(ApplicationUserId, ApplicationGroupId) VALUES");
                var rows = new List<string>();

                // iterate to each claim and add it as row
                foreach (var groupId in groupIds)
                {
                    rows.Add(string.Format("(\'{0}\', \'{1}\')", userId, groupId));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }
        }

        /// <summary>
        /// Add group id to the list of roles
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <param name="roleIds">The list of role ids</param>
        public void AddGroupRoles(string groupId, params string[] roleIds)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var sCommand = new StringBuilder("INSERT INTO applicationgrouproles(ApplicationGroupId, RoleId) VALUES");
                var rows = new List<string>();

                // iterate to each claim and add it as row
                foreach (var roleId in roleIds)
                {
                    rows.Add(string.Format("(\'{0}\', \'{1}\')", groupId, roleId));
                }

                sCommand.Append(string.Join(",", rows));
                sCommand.Append(";");

                MySqlHelper.ExecuteNonQuery(conn, sCommand.ToString(), null);
            }
        }

        /// <summary>
        /// Delete the roles assigned to the group id
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        public void DeleteGroupRoles(string groupId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@ApplicationGroupId", groupId}
                };

                MySqlHelper.ExecuteNonQuery(conn,
                    @"DELETE FROM applicationgrouproles WHERE ApplicationGroupId = @ApplicationGroupId", parameters);
            }
        }

        /// <summary>
        /// Delete the groups assigned to the role id
        /// </summary>
        /// <param name="roleId">The id of the role</param>
        public void DeleteRoleGroups(string roleId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@RoleId", roleId}
                };

                MySqlHelper.ExecuteNonQuery(conn,
                    @"DELETE FROM applicationgrouproles WHERE RoleId = @RoleId", parameters);
            }
        }

        /// <summary>
        /// Delete users assigned to the specified group id
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        public void DeleteGroupUsers(string groupId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@ApplicationGroupId", groupId}
                };

                MySqlHelper.ExecuteNonQuery(conn,
                    @"DELETE FROM applicationusergroups WHERE ApplicationGroupId = @ApplicationGroupId", parameters);
            }
        }

        /// <summary>
        /// Remove the user from its current groups membership
        /// </summary>
        /// <param name="userId">The user id</param>
        public void DeleteUserGroups(string userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@ApplicationUserId", userId}
                };

                MySqlHelper.ExecuteNonQuery(conn,
                    @"DELETE FROM applicationusergroups WHERE ApplicationUserId = @ApplicationUserId", parameters);
            }
        }

        public List<ApplicationGroupRole> PopulateGroupRoles(string groupId)
        {
            var groupRoles = new List<ApplicationGroupRole>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@groupId", groupId}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT ApplicationGroupId,RoleId FROM applicationgrouproles WHERE ApplicationGroupId = @groupId",
                    parameters);
                while (reader.Read())
                {
                    var groupRole = new ApplicationGroupRole();
                    groupRole.ApplicationGroupId = reader[0].ToString();
                    groupRole.RoleId = reader[1].ToString();
                    groupRoles.Add(groupRole);
                }
            }
            return groupRoles;
        }

        public List<ApplicationUserGroup> PopulateUserGroups(string groupId)
        {
            var userGroups = new List<ApplicationUserGroup>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@groupId", groupId}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT ApplicationUserId,ApplicationGroupId FROM applicationusergroups WHERE ApplicationGroupId = @groupId",
                    parameters);
                while (reader.Read())
                {
                    var userGroup = new ApplicationUserGroup();
                    userGroup.ApplicationUserId = reader[0].ToString();
                    userGroup.ApplicationGroupId = reader[1].ToString();
                    userGroups.Add(userGroup);
                }
            }
            return userGroups;
        }
    }
}
