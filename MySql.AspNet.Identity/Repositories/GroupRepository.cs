using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace MySql.AspNet.Identity.Repositories
{
    public class GroupRepository
    {
        private readonly string _connectionString;
        public GroupRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// Get the list of application groups
        /// </summary>
        /// <returns></returns>
        public IQueryable<ApplicationGroup> GetGroups()
        {
            var groups = new List<ApplicationGroup>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT g.Id, g.Name, g.Description, COUNT(ug.ApplicationUserId) AS NumberOfUsers
                    FROM applicationgroups g
                    LEFT JOIN applicationusergroups ug
	                    ON ug.ApplicationGroupId = g.Id
                    GROUP BY g.Id, g.Name, g.Description
                    ORDER BY g.Name;", null);

                while (reader.Read())
                {
                    var group = new ApplicationGroup();
                    group.Id = reader[0].ToString();
                    group.Name = reader[1].ToString();
                    group.Description = reader[2].ToString();
                    group.NumberOfUsers = Convert.ToInt32(reader[3]);

                    groups.Add(group);
                }
            }

            return groups.AsQueryable();
        }

        /// <summary>
        /// Get the application group with the specified id
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <returns></returns>
        public ApplicationGroup GetGroupById(string groupId)
        {
            var group = new ApplicationGroup();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@id", groupId}
                };
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, @"SELECT Name,Description FROM applicationgroups WHERE Id = @id",
                    parameters);

                while (reader.Read())
                {
                    group.Name = reader[0].ToString();
                    group.Description = reader[1].ToString();
                    group.Id = groupId;
                }
            }

            return group;
        }

        /// <summary>
        /// Get the group with the specified name
        /// </summary>
        /// <param name="groupName">The name of the group</param>
        /// <returns></returns>
        public ApplicationGroup GetGroupByName(string groupName)
        {
            var groupId = getGroupId(groupName);
            ApplicationGroup group = null;

            if (groupId != null)
                group = new ApplicationGroup(groupId, groupName);

            return group;
        }

        /// <summary>
        /// Insert a new group to database
        /// </summary>
        /// <param name="group">The object to save</param>
        public void Insert(ApplicationGroup group)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@description", group.Description},
                    {"@name", group.Name},
                    {"@id", group.Id}
                };

                MySqlHelper.ExecuteNonQuery(conn, @"INSERT INTO applicationgroups (Id,Name,Description) VALUES (@id,@name,@description)", parameters);
            }
        }

        /// <summary>
        /// Delete the group with the specified id
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        public void Delete(string groupId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@id", groupId}
                };

                // deleter first the application user group
                MySqlHelper.ExecuteNonQuery(conn, @"DELETE FROM applicationusergroups WHERE ApplicationGroupId = @id", parameters);

                // next the application group roles
                MySqlHelper.ExecuteNonQuery(conn, @"DELETE FROM applicationgrouproles WHERE ApplicationGroupId = @id", parameters);

                // lastly, the main application group
                MySqlHelper.ExecuteNonQuery(conn, @"DELETE FROM applicationgroups WHERE Id = @id", parameters);
            }
        }

        /// <summary>
        /// Update the group details
        /// </summary>
        /// <param name="group">The updated group data</param>
        public void Update(ApplicationGroup group)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@description", group.Description},
                    {"@name", group.Name},
                    {"@id", group.Id}
                };

                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE applicationgroups SET Name = @name, Description = @description WHERE Id = @id", parameters);
            }
        }

        private string getGroupId(string groupName)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>()
                {
                    {"@name", groupName}
                };

                var result = MySqlHelper.ExecuteScalar(conn, CommandType.Text, @"SELECT Id FROM applicationgroups WHERE Name = @name", parameters);
                if (result != null)
                {
                    return result.ToString();
                }
            }

            return null;
        }
    }
}
