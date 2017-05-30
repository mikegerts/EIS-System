using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MySql.Data.MySqlClient;

namespace MySql.AspNet.Identity.Repositories
{
    public class UserRepository<TUser> where TUser : IdentityUser
    {
        private readonly string _connectionString;
        public UserRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Insert(TUser user)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@Id", user.Id},
                    {"@Email", (object) user.Email ?? DBNull.Value},
                    {"@EmailConfirmed", user.EmailConfirmed},
                    {"@PasswordHash", (object) user.PasswordHash ?? DBNull.Value},
                    {"@SecurityStamp", (object) user.SecurityStamp ?? DBNull.Value},
                    {"@PhoneNumber", (object) user.PhoneNumber ?? DBNull.Value},
                    {"@PhoneNumberConfirmed", user.PhoneNumberConfirmed},
                    {"@TwoFactorAuthEnabled", user.TwoFactorAuthEnabled},
                    {"@LockoutEndDate", (object) user.LockoutEndDate ?? DBNull.Value},
                    {"@LockoutEnabled", user.LockoutEnabled},
                    {"@AccessFailedCount", user.AccessFailedCount},
                    {"@UserName", user.UserName},
                    {"@FirstName", user.FirstName},
                    {"@LastName", user.LastName},
                    {"@Website", user.Website},
                    {"@AddressLine1", user.AddressLine1},
                    {"@AddressLine2", user.AddressLine2},
                    {"@State", user.State},
                    {"@ZipCode", user.ZipCode},
                    {"@LastLoginDate", user.LastLoginDate},
                    {"@Created", DateTime.UtcNow}
                };

                MySqlHelper.ExecuteNonQuery(conn, @"INSERT INTO aspnetusers VALUES(@Id,@Email,@EmailConfirmed,@PasswordHash,@SecurityStamp,@PhoneNumber,@PhoneNumberConfirmed,
                @TwoFactorAuthEnabled,@LockoutEndDate,@LockoutEnabled,@AccessFailedCount,@UserName,@FirstName,@LastName,
                @Website,@AddressLine1,@AddressLine2,@State,@ZipCode,@LastLoginDate,@Created)", parameters);
            }
        }

        public void Delete(TUser user)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@Id", user.Id}
                };

                MySqlHelper.ExecuteNonQuery(conn, @"DELETE FROM aspnetusers WHERE Id=@Id", parameters);
            }
        }

        public IQueryable<TUser> GetAll()
        {
            List<TUser> users = new List<TUser>();

            using (var conn = new MySqlConnection(_connectionString))
            {
                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT Id,Email,EmailConfirmed,
                PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,
                LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,FirstName,LastName,
                Website,AddressLine1,AddressLine2,State,ZipCode,LastLoginDate,Created FROM aspnetusers", null);
                
                while (reader.Read())
                {
                    var user = (TUser)Activator.CreateInstance(typeof(TUser));
                    user.Id = reader[0].ToString();
                    user.Email = reader[1].ToString();
                    user.EmailConfirmed = (bool)reader[2];
                    user.PasswordHash = reader[3].ToString();
                    user.SecurityStamp = reader[4].ToString();
                    user.PhoneNumber = reader[5].ToString();
                    user.PhoneNumberConfirmed = (bool)reader[6];
                    user.TwoFactorAuthEnabled = (bool)reader[7];
                    user.LockoutEndDate = reader[8] == DBNull.Value ? null : (DateTime?)reader[8];
                    user.LockoutEnabled = (bool)reader[9];
                    user.AccessFailedCount = (int)reader[10];
                    user.UserName = reader[11].ToString();
                    user.FirstName = reader[12].ToString();
                    user.LastName = reader[13].ToString();
                    user.Website = reader[14].ToString();
                    user.AddressLine1 = reader[15].ToString();
                    user.AddressLine2 = reader[16].ToString();
                    user.State = reader[17].ToString();
                    user.ZipCode = reader[18].ToString();
                    user.LastLoginDate = reader[19] == DBNull.Value ? null : (DateTime?)reader[19];
                    user.Created = reader[20] == DBNull.Value ? null : (DateTime?)reader[20];

                    users.Add(user);
                }

            }
            return users.AsQueryable<TUser>();
        }
        
        public TUser GetById(string userId)
        {
            var user = (TUser)Activator.CreateInstance(typeof(TUser));
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@Id", userId}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT Id,Email,EmailConfirmed,
                PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,
                LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,FirstName,LastName,
                Website,AddressLine1,AddressLine2,State,ZipCode,LastLoginDate,Created FROM aspnetusers WHERE Id=@Id", parameters);
                while (reader.Read())
                {
                    user.Id = reader[0].ToString();
                    user.Email = reader[1].ToString();
                    user.EmailConfirmed = (bool)reader[2];
                    user.PasswordHash = reader[3].ToString();
                    user.SecurityStamp = reader[4].ToString();
                    user.PhoneNumber = reader[5].ToString();
                    user.PhoneNumberConfirmed = (bool)reader[6];
                    user.TwoFactorAuthEnabled = (bool)reader[7];
                    user.LockoutEndDate = reader[8] == DBNull.Value ? null : (DateTime?)reader[8];
                    user.LockoutEnabled = (bool)reader[9];
                    user.AccessFailedCount = (int)reader[10];
                    user.UserName = reader[11].ToString();
                    user.FirstName = reader[12].ToString();
                    user.LastName = reader[13].ToString();
                    user.Website = reader[14].ToString();
                    user.AddressLine1 = reader[15].ToString();
                    user.AddressLine2 = reader[16].ToString();
                    user.State = reader[17].ToString();
                    user.ZipCode = reader[18].ToString();
                    user.LastLoginDate = reader[19] == DBNull.Value ? null : (DateTime?)reader[19];
                    user.Created = reader[20] == DBNull.Value ? null : (DateTime?)reader[20];
                }

            }
            return user;
        }

        public TUser GetByName(string userName)
        {
            var user = (TUser)Activator.CreateInstance(typeof(TUser));
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@UserName", userName}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT Id,Email,EmailConfirmed,
                PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,
                LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,FirstName,LastName,
                Website,AddressLine1,AddressLine2,State,ZipCode,LastLoginDate,Created FROM aspnetusers WHERE UserName=@UserName", parameters);
                while (reader.Read())
                {
                    user.Id = reader[0].ToString();
                    user.Email = reader[1].ToString();
                    user.EmailConfirmed = (bool)reader[2];
                    user.PasswordHash = reader[3].ToString();
                    user.SecurityStamp = reader[4].ToString();
                    user.PhoneNumber = reader[5].ToString();
                    user.PhoneNumberConfirmed = (bool)reader[6];
                    user.TwoFactorAuthEnabled = (bool)reader[7];
                    user.LockoutEndDate = reader[8] == DBNull.Value ? null : (DateTime?)reader[8];
                    user.LockoutEnabled = (bool)reader[9];
                    user.AccessFailedCount = (int)reader[10];
                    user.UserName = reader[11].ToString();
                    user.FirstName = reader[12].ToString();
                    user.LastName = reader[13].ToString();
                    user.Website = reader[14].ToString();
                    user.AddressLine1 = reader[15].ToString();
                    user.AddressLine2 = reader[16].ToString();
                    user.State = reader[17].ToString();
                    user.ZipCode = reader[18].ToString();
                    user.LastLoginDate = reader[19] == DBNull.Value ? null : (DateTime?)reader[19];
                    user.Created = reader[20] == DBNull.Value ? null : (DateTime?)reader[20];
                }

            }
            return user;
        }

        public TUser GetByEmail(string email)
        {
            var user = (TUser)Activator.CreateInstance(typeof(TUser));
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@Email", email}
                };

                var reader = MySqlHelper.ExecuteReader(conn, CommandType.Text,
                    @"SELECT Id,Email,EmailConfirmed,
                PasswordHash,SecurityStamp,PhoneNumber,PhoneNumberConfirmed,TwoFactorEnabled,
                LockoutEndDateUtc,LockoutEnabled,AccessFailedCount,UserName,FirstName,LastName,
                Website,AddressLine1,AddressLine2,State,ZipCode,LastLoginDate,Created FROM aspnetusers WHERE Email=@Email", parameters);
                while (reader.Read())
                {
                    user.Id = reader[0].ToString();
                    user.Email = reader[1].ToString();
                    user.EmailConfirmed = (bool)reader[2];
                    user.PasswordHash = reader[3].ToString();
                    user.SecurityStamp = reader[4].ToString();
                    user.PhoneNumber = reader[5].ToString();
                    user.PhoneNumberConfirmed = (bool)reader[6];
                    user.TwoFactorAuthEnabled = (bool)reader[7];
                    user.LockoutEndDate = reader[8] == DBNull.Value ? null : (DateTime?)reader[8];
                    user.LockoutEnabled = (bool)reader[9];
                    user.AccessFailedCount = (int)reader[10];
                    user.UserName = reader[11].ToString();
                    user.FirstName = reader[12].ToString();
                    user.LastName = reader[13].ToString();
                    user.Website = reader[14].ToString();
                    user.AddressLine1 = reader[15].ToString();
                    user.AddressLine2 = reader[16].ToString();
                    user.State = reader[17].ToString();
                    user.ZipCode = reader[18].ToString();
                    user.LastLoginDate = reader[19] == DBNull.Value ? null : (DateTime?)reader[19];
                    user.Created = reader[20] == DBNull.Value ? null : (DateTime?)reader[20];
                }

            }
            return user;
        }

        public void UpdateLastLoginDate(string userId)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@LastLoginDate", DateTime.UtcNow},
                    {"@Id", userId}
                };

                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE aspnetusers 
                    SET LastLoginDate=@LastLoginDate
                    WHERE Id=@Id", parameters);
            }
        }       

        public void Update(TUser user)
        {
            using (var conn = new MySqlConnection(_connectionString))
            {
                var parameters = new Dictionary<string, object>
                {
                    {"@NewId", user.Id},
                    {"@Email", (object) user.Email ?? DBNull.Value},
                    {"@EmailConfirmed", user.EmailConfirmed},
                    {"@PasswordHash", (object) user.PasswordHash ?? DBNull.Value},
                    {"@SecurityStamp", (object) user.SecurityStamp ?? DBNull.Value},
                    {"@PhoneNumber", (object) user.PhoneNumber ?? DBNull.Value},
                    {"@PhoneNumberConfirmed", user.PhoneNumberConfirmed},
                    {"@TwoFactorAuthEnabled", user.TwoFactorAuthEnabled},
                    {"@LockoutEndDate", (object) user.LockoutEndDate ?? DBNull.Value},
                    {"@LockoutEnabled", user.LockoutEnabled},
                    {"@AccessFailedCount", user.AccessFailedCount},
                    {"@UserName", user.UserName},
                    {"@FirstName", user.FirstName},
                    {"@LastName", user.LastName},
                    {"@Website", user.Website},
                    {"@AddressLine1", user.AddressLine1},
                    {"@AddressLine2", user.AddressLine2},
                    {"@State", user.State},
                    {"@ZipCode", user.ZipCode},
                    {"@LastLoginDate", user.LastLoginDate},
                    {"@Id", user.Id}
                };

                MySqlHelper.ExecuteNonQuery(conn, @"UPDATE aspnetusers 
                SET Id = @NewId,Email=@Email,EmailConfirmed=@EmailConfirmed,PasswordHash=@PasswordHash,SecurityStamp=@SecurityStamp,PhoneNumber=@PhoneNumber,PhoneNumberConfirmed=@PhoneNumberConfirmed,
                TwoFactorEnabled=@TwoFactorAuthEnabled,LockoutEndDateUtc=@LockoutEndDate,LockoutEnabled=@LockoutEnabled,AccessFailedCount=@AccessFailedCount,UserName=@UserName,FirstName=@FirstName,LastName=@LastName,
                Website=@Website,AddressLine1=@AddressLine1,AddressLine2=@AddressLine2,State=@State,ZipCode=@ZipCode,LastLoginDate=@LastLoginDate 
                WHERE Id=@Id", parameters);
            }
        }
    }
}
