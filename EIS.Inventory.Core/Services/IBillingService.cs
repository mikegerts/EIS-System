using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;
using System;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public interface IBillingService
    {
        /// <summary>
        /// Get the list of purchse orders
        /// </summary>
        /// <param name="page"></param>
        /// <param name="pageSize"></param>
        /// <param name="vendorId"></param>
        /// <param name="paymentStatus"></param>
        /// <param name="fromDate"></param>
        /// <param name="toDate"></param>
        /// <returns></returns>
        IEnumerable<PurchaseOrderViewModel> GetPurchaseOrders(int page, int pageSize, int vendorId, PaymentStatus paymentStatus, DateTime? fromDate, DateTime? toDate);

        /// <summary>
        /// Get the list of purchase order that satisfy with the specified search criteria
        /// </summary>
        /// <param name="searchStr">The string to search</param>
        /// <returns></returns>
        IEnumerable<PurchaseOrderViewModel> GetPurchaseOrdersContainsBy(string searchStr);

        /// <summary>
        /// Get the purchase order detail with the specified id
        /// </summary>
        /// <param name="id">The id of the purchase order</param>
        /// <returns></returns>
        PurchaseOrderViewModel GetPurchaseOrder(string id);

        /// <summary>
        /// Get the list of purchase order items with the specified purchase order id
        /// </summary>
        /// <param name="poId">The id of the purchase order</param>
        /// <returns></returns>
        IEnumerable<PurchaseOrderItem> GetPurchaseOrderItems(string poId);

        /// <summary>
        /// Update the purchase order item to mark as paid
        /// </summary>
        /// <param name="poId">The id of the Purchase Order</param>
        /// <param name="paidPoItems">The items to be updated</param>
        /// <param name="unpaidPoItems">The PO items that are unpaid</param>
        /// <returns></returns>
        PurchaseOrderViewModel UpdatePurchaseOrderItems(string poId, List<long> paidPoItems ,List<long> unpaidPoItems);

        /// <summary>
        /// Save the Purchae Order to the database
        /// </summary>
        /// <param name="model">The purchase order to save</param>
        PurchaseOrderViewModel SavePurchaseOrder(PurchaseOrderViewModel model);

        /// <summary>
        /// Update the Purchase Order
        /// </summary>
        /// <param name="poId">The Purhase Order id to update</param>
        /// <param name="model">The updated Purchase Order</param>
        PurchaseOrderViewModel UpdatePurchaseOrderItems(string poId, PurchaseOrderViewModel model);

        /// <summary>
        /// Delete bulk billings
        /// </summary>
        /// <param name="isSelectAllPages"></param>
        /// <param name="billingIds"></param>
        void DeleteBillings(bool isSelectAllPages, List<string> billingIds);
    }
}
