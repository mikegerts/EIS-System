using EIS.Inventory.Shared.Models;
using EIS.SystemJobApp.Repositories;

namespace EIS.SystemJobApp.Managers
{
    public class VendorProductDeleteManager : IBulkDeleteManager
    {
        private readonly VendorProductRepository _repo;

        public VendorProductDeleteManager(LoggerRepository logger)
        {
            _repo = new VendorProductRepository(logger);
        }

        public JobType JobType
        {
            get { return JobType.BulkDeleteVendorProduct; }
        }

        public int DeleteItem(string id)
        {
            return _repo.DeleteVendorProduct(id);
        }
    }
}
