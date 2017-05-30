using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using EIS.Console.DAL.Database;
using MySql.Data.MySqlClient;

namespace EIS.Console.DAL.Repository
{
	public enum FileType
	{
		CSV = 0,
		Excel = 1
	};

	public class VendorsConfig
	{

		public static async Task<IList<vendor>> GetScheduledVendors()
		{
			var results = new List<vendor>();

			using (var db = new VendorsConfigEntities())
			{
				using (var con = db.Database.Connection)
				{
					var cmd = con.CreateCommand();
					con.Open();

               cmd.CommandText = @"
	select v.*, u.Id as StatusId, u.VendorId, u.StartUploadDate, u.EndUploadDate, u.Status, u.Attempt from vendors v
	left join uploadstatus u on 
		(	
			v.Id = u.VendorId 
			and 
			u.Id = (select max(Id) from uploadstatus where VendorId = v.Id)
		)
	where
		isnull(u.StartUploadDate) -- is uploaded from start
		or
		(
			(
				u.StartUploadDate < concat(curdate(), ' ', v.ReadTime) -- is uploaded today
				or
				(u.Status = 0 and u.Attempt < 3)-- is error occured
			)
			and
			concat(curdate(), ' ', v.ReadTime) <= now() -- is schedule to upload
		)
	order by v.Id asc
					";

					var dr = await cmd.ExecuteReaderAsync();

					while (dr.Read())
					{
						var v = new vendor
						{
							Id = Convert.ToInt32(dr["Id"]),
							VendorName = Convert.ToString(dr["VendorName"]),
							FileName = Convert.ToString(dr["FileName"]),
							FilePath = Convert.ToString(dr["FilePath"]),
							TransferPath = Convert.ToString(dr["TransferPath"]),
							EISSKUCode = Convert.ToString(dr["EISSKUCode"]),
							UploadTime = TimeSpan.Parse(dr["UploadTime"].ToString()),
							ReadTime = TimeSpan.Parse(dr["ReadTime"].ToString()),
							RowAt = Convert.ToInt32(dr["RowAt"]),
							FileType = Convert.ToString(dr["FileType"])
						};

						if (dr["StatusId"] != DBNull.Value)
						{
							v.uploadstatus.Add(new uploadstatu
							{
								Id = Convert.ToInt32(dr["StatusId"]),
								VendorId = Convert.ToInt32(dr["VendorId"]),
								StartUploadDate = Convert.ToDateTime(dr["StartUploadDate"]),
								EndUploadDate = (dr["EndUploadDate"] != DBNull.Value) ? Convert.ToDateTime(dr["EndUploadDate"]) : new DateTime(),
								Status = Convert.ToBoolean(dr["Status"]),
								Attempt = (dr["Attempt"] != DBNull.Value) ? Convert.ToInt32(dr["Attempt"]) : 0
							});
							
						}

						results.Add(v);
					}

					con.Close();
				}

				return results;
			}
		}

		public static async Task DeleteCurrentProductsByVendorId(int vendorId)
		{
			using (var db = new VendorsConfigEntities())
			{
				using (var con = db.Database.Connection)
				{
					var cmd = con.CreateCommand();
					cmd.CommandText = @"delete from vendorproducts where ResultDate = curdate() and VendorId = @VendorId";
					cmd.Parameters.Add(new MySqlParameter("@VendorId", vendorId));
					con.Open();

					await cmd.ExecuteNonQueryAsync();

					con.Close();
				}
			}
		}

		public static async Task<productfileconfig> GetProductFileConfigByVendorId(int vendorId)
		{
			using (var db = new VendorsConfigEntities())
			{
				return await db.productfileconfigs.FirstOrDefaultAsync(m => m.VendorId == vendorId);
			}
		}

		public static async Task<IList<imagefileconfig>> GetImageFileConfigByVendorId(int vendorId)
		{
			using (var db = new VendorsConfigEntities())
			{
				return await db.imagefileconfigs.Where(m => m.VendorId == vendorId).ToListAsync();
			}
		}

		public static async Task<long> AddVendorProduct(vendorproduct product)
		{
			using (var db = new VendorsConfigEntities())
			{
				db.vendorproducts.Add(product);
				await db.SaveChangesAsync();

				return product.Id;
			}
		}

		public static async Task<long> AddProductImage(productimage image)
		{
			using (var db = new VendorsConfigEntities())
			{
				db.productimages.Add(image);
				await db.SaveChangesAsync();

				return image.Id;
			}
		}

		public static async Task<long> SaveUploadStatus(uploadstatu status)
		{
			using (var db = new VendorsConfigEntities())
			{
				if (status.Id != 0)
				{
					db.Entry(status).State = EntityState.Modified;
				}
				else
				{
					db.uploadstatus.Add(status);
				}

				await db.SaveChangesAsync();

				return status.Id;
			}
		}

		public static async Task<uploadstatu> CreateUploadStatus(int vendorId)
		{
			var status = new uploadstatu { VendorId = vendorId, StartUploadDate = DateTime.Now, Attempt = 0 };

			await SaveUploadStatus(status);

			return status;
		}

		public static async Task UpdateUploadStatus(uploadstatu uploadstatus, bool status)
		{
			uploadstatus.Attempt = (++uploadstatus.Attempt) ?? 0;
			uploadstatus.EndUploadDate = DateTime.Now;
			uploadstatus.Status = status;

			await SaveUploadStatus(uploadstatus);
		}
	}
}
