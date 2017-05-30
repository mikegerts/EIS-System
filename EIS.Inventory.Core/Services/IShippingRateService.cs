using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EIS.Inventory.Core.ViewModels;

namespace EIS.Inventory.Core.Services {

    public interface IShippingRateService {
        /// <summary>
        /// Get the list of all shipping rates
        /// </summary>
        /// <returns></returns>
        IEnumerable<ShippingRateDto> GetAllShippingRates ();

        /// <summary>
        /// Get the shipping rate with the specified shipping rate id
        /// </summary>
        /// <param name="shippingRateId">The shipping rate Id</param>
        /// <returns></returns>
        ShippingRateDto GetShippingRate ( int shippingRateId );

        /// <summary>
        /// Create new shipping rate in the database
        /// </summary>
        /// <param name="model">The shipping rate to save</param>
        /// <returns></returns>
        ShippingRateDto CreateShippingRate ( ShippingRateDto model );

        /// <summary>
        /// Update the shipping rate with the modified model
        /// </summary>
        /// <param name="model">The updated shipping rate</param>
        /// <returns></returns>
        ShippingRateDto UpdateShippingRatey ( ShippingRateDto model );

        /// <summary>
        /// Delete the shipping rate with the specified shipping rate Id
        /// </summary>
        /// <param name="shippingRateId">The shipping rate Id</param>
        /// <returns></returns>
        bool DeleteShippingRate ( int shippingRateId );
    }
}
