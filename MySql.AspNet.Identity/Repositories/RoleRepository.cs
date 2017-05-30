using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace MySql.AspNet.Identity.Repositories
{
    public class RoleRepository<TRole> where TRole: IdentityRole
    {
        private readonly string _connectionString;
        public RoleRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IQueryable<TRole> GetRoles()
        {
            var roles = new List<TRole>();
            using (var conn = new MySqlConnection(_connectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, @"SELECT Id,Name,`Description`,`Order` FROM aspnetroles ORDER BY `Order`", null);

                while (reader.Read())
                {
                    var role = (TRole)Activator.CreateInstance(typeof(TRole));

                    role.Id = reader[0].ToString();
                    role.Name = reader[1].ToString();
                    role.Description = reader[2].ToString();
                    role.Order = Convert.ToInt32(reader[3]);

                    roles.Add(role);
                }

            }

            return roles.AsQueryable();
        }

        public IdentityRole GetRoleById(string roleId)
        {
            IdentityRole role = null;
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@id", roleId}
                };
                role = new IdentityRole();
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text, @"SELECT Id,Name,`Description`,`Order` FROM aspnetroles WHERE Id = @id", parameters);

                while (reader.Read())
                {
                    role.Id = reader[0].ToString();
                    role.Name = reader[1].ToString();
                    role.Description = reader[2].ToString();
                    role.Order = Convert.ToInt32(reader[3]);
                }
            }

            return role;
        }

        public IdentityRole GetRoleByName(string roleName)
        {
            var roleId = getRoleId(roleName);
            IdentityRole role = null;

            if (roleId != null)
            {
                role = new IdentityRole(roleName, roleId);
            }

            return role;
        }

        public int GetMaxOrder()
        {
            var maxOrder = 0;
            using (var conn = new MySqlConnection(_connectionString))
            {
                var result = MySqlHelper.ExecuteScalar(conn, CommandType.Text, @"SELECT MAX(`Order`) FROM aspnetroles", null);
                if (result != null)
                {
                    maxOrder = Convert.ToInt32(result);
                }
            }

            return maxOrder;
        }
        
        public void Insert(IdentityRole role)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@name", role.Name },
                    {"@id", role.Id },
                    {"@description", role.Description },
                    {"@order", role.Order }
                };

                MySqlHelper.ExecuteNonQuery(conn, @"INSERT INTO aspnetroles (Id, Name, `Description`, `Order`) VALUES (@id, @name, @description, @order)", parameters);
            }
        }

        public void Update(IdentityRole role)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@name", role.Name},
                    {"@description", role.Description},
                    {"@order", role.Order},
                    {"@id", role.Id}
                };

                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE aspnetroles SET Name = @name, `Description` = @description, `Order`= @order WHERE Id = @id", parameters);
            }
        }

        public void Delete(string roleId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@id", roleId}
                };

                MySqlHelper.ExecuteNonQuery(conn, @"DELETE FROM aspnetroles WHERE Id = @id", parameters);
            }
        }

        private string getRoleId(string roleName)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>()
                {
                    {"@name", roleName}
                };

                var result = MySqlHelper.ExecuteScalar(conn, CommandType.Text, @"SELECT Id FROM aspnetroles WHERE Name = @name", parameters);
                if (result != null)
                {
                    return result.ToString();
                }
            }

            return null;
        }
    }
}
