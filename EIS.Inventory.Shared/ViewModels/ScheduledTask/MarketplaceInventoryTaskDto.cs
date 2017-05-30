using System.Collections.Generic;

namespace EIS.Inventory.Shared.ViewModels
{
    public class MarketplaceInventoryTaskDto : ScheduledTaskDto
    {
        public MarketplaceInventoryTaskDto()
        {
            Marketplaces = new List<string>();
        }

        /// <summary>
        /// Gets or sets the company id
        /// </summary>
        public int CompanyId { get; set; }

        /// <summary>
        /// Gets or sets the list of name of the markeplaces
        /// </summary>
        public List<string> Marketplaces { get; set; }
    }
}
