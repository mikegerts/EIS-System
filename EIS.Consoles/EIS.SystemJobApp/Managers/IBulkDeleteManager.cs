using EIS.Inventory.Shared.Models;

namespace EIS.SystemJobApp.Managers
{
    public interface IBulkDeleteManager
    {
        /// <summary>
        /// Get the name of the job type
        /// </summary>
        JobType JobType { get; }

        /// <summary>
        /// Delete the item with the specified ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        int DeleteItem(string id);
    }
}
