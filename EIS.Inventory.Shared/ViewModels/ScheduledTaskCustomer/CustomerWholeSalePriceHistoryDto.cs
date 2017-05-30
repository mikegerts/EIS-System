using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.ViewModels
{
    public class CustomerWholeSalePriceHistoryDto
    {
        /// <summary>
        /// Gets or sets the id of the task
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the EisSKU of the task
        /// </summary>
        public string EisSKU { get; set; }

        /// <summary>
        /// Gets or sets the CustomerId of the task
        /// </summary>
        public int CustomerId { get; set; }

        /// <summary>
        /// Gets or sets the CustomerScheduleId of the task
        /// </summary>
        public int CustomerScheduleId { get; set; }

        /// <summary>
        /// Gets or sets the SkuCalculatedPrice of the task
        /// </summary>
        public decimal SkuCalculatedPrice { get; set; }

        /// <summary>
        /// Gets or sets the ModifiedBy of the task
        /// </summary>
        public string ModifiedBy { get; set; }


        /// <summary>
        /// Gets or sets the CreatedBy of the task
        /// </summary>
        public string CreatedBy { get; set; }


        /// <summary>
        /// Gets or sets the Modified of the task
        /// </summary>
        public DateTime Modified { get; set; }


        /// <summary>
        /// Gets or sets the Created of the task
        /// </summary>
        public DateTime Created { get; set; }





    }
}