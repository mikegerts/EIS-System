using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace EIS.Inventory.DAL.Database
{
    public partial class EisInventoryContext : DbContext
    {
        public EisInventoryContext(string connectionStringName)
            : base(connectionStringName)
        {
        }
    }
}
