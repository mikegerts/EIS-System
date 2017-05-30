using EIS.Inventory.Shared.ViewModels;
using System;
using System.Collections.Generic;
using X.PagedList;

namespace EIS.Inventory.Core.Services
{
    public interface IKitService : IDisposable
    {
        /// <summary>
        /// Get the Kit by EIS SKU
        /// </summary>
        /// <param name="parentKitSKU"></param>
        /// <returns></returns>
        KitDto GetKitByParentKitSku(string parentKitSKU);

        /// <summary>
        /// Get the kit detail with the specified IDs
        /// </summary>
        /// <param name="parentKitSku"></param>
        /// <param name="childKitSku"></param>
        /// <returns></returns>
        KitDetailDto GetKitDetailByIds(string parentKitSku, string childKitSku);

        /// <summary>
        /// Get the list of kit details by kit Id
        /// </summary>
        /// <param name="parentKitSku"></param>
        /// <param name="childKitSku"></param>
        /// <returns></returns>
        IEnumerable<KitDetailDto> GetKitDetailsByParentKitSku(string parentKitSku);

        /// <summary>
        /// Get the list of Kits containing with the search keywords
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="searchStr"></param>
        /// <returns></returns>
        IPagedList<ProductSearchDto> GetProducts(int page, int pageSize, string searchStr);

        /// <summary>
        /// Update the kit information
        /// </summary>
        /// <param name="parentKitSku"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        KitDto UpdateKit(string parentKitSku, KitDto model);

        /// <summary>
        /// Update the kit detail with the specified udpated model object
        /// </summary>
        /// <param name="kitDetailId"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        KitDetailDto UpdateKitDetail(KitDetailDto model);

        /// <summary>
        /// Updae the kit details to the specified kit id
        /// </summary>
        /// <param name="parentKitSKU"></param>
        /// <param name="models"></param>
        /// <returns></returns>
        KitDto SaveKitDetails(string parentKitSKU, List<KitDetailDto> models);

        /// <summary>
        /// Delete the kit detail
        /// </summary>
        /// <param name="parentKitSku"></param>
        /// <returns></returns>
        bool DeleteKitDetail(string parentKitSku, string childKitSku);
    }
}
