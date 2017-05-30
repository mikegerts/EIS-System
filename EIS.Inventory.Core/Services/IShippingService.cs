using System;
using X.PagedList;
using EIS.Inventory.Shared.ViewModels;
using System.Collections.Generic;

namespace EIS.Inventory.Core.Services
{
    public interface IShippingService : IDisposable
    {
        /// <summary>
        ///  Get the list of order product items which are need to be shipped
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        IPagedList<OrderProductListDto> GetAwaitingShipments(int page, int pageSize);

        /// <summary>
        /// Get the order product details with the specified order idS
        /// </summary>
        /// <param name="orderId">The marketplace order id</param>
        /// <returns></returns>
        OrderProductDetailDto GetOrderProductDetailByOrderId(string orderId);

        /// <summary>
        /// Get the paginated list of shipping locations
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        IPagedList<ShippingLocationDto> GetShippingLocations(int page, int pageSize);

        /// <summary>
        /// Get all the list for shipping locations
        /// </summary>
        /// <returns></returns>
        List<ShippingLocationDto> GetAllShippingLocations();

        /// <summary>
        /// Get the shipping location data
        /// </summary>
        /// <param name="id">The id of the shipping location</param>
        /// <returns></returns>
        ShippingLocationDto GetShippingLocationById(int id);

        /// <summary>
        /// Save the shipping location to the database
        /// </summary>
        /// <param name="model">The model to save</param>
        /// <returns></returns>
        bool CreateShippingLocation(ShippingLocationDto model);

        /// <summary>
        /// Update the shipping location with the specified updated model data
        /// </summary>
        /// <param name="id">The id of shipping location</param>
        /// <param name="model">The updated model</param>
        /// <returns></returns>
        bool UpdateShippingLocation(int id, ShippingLocationDto model);

        /// <summary>
        /// Delete the shipping location with the specified id
        /// </summary>
        /// <param name="id">The id of shipping location</param>
        /// <returns></returns>
        void DeleteShippingLocation(int id);
    }
}
