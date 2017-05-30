using EIS.Inventory.Shared.Models;
using EIS.SystemJobApp.Repositories;

namespace EIS.SystemJobApp.Managers
{
    public class ProductDeleteManager : IBulkDeleteManager
    {
        private readonly ProductRepository _repo;

        public ProductDeleteManager(LoggerRepository logger)
        {
            _repo = new ProductRepository(logger);
        }

        public JobType JobType
        {
            get { return JobType.BulkDeleteProduct; }
        }

        public int DeleteItem(string id)
        {
            return _repo.DeleteProduct(id);
        }
    }
}
