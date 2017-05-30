using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.Services
{
    public class BillingService : IBillingService, IDisposable
    {
        private bool _disposed;
        private EisInventoryContext _context;

        public BillingService()
        {
            _context = new EisInventoryContext();
        }

        public IEnumerable<PurchaseOrderViewModel> GetPurchaseOrders(int page,
            int pageSize,
            int vendorId,
            PaymentStatus paymentStatus,
            DateTime? fromDate,
            DateTime? toDate)
        {           
            var orders = _context.purchaseorders
                   .Where(x => (vendorId == -1 || x.VendorId == vendorId)
                       && (paymentStatus == PaymentStatus.All || x.PaymentStatus == paymentStatus)
                       && (fromDate == null || (x.Created >= fromDate && x.Created <= toDate)))
                   .OrderByDescending(x => x.Created);

            return Mapper.Map<IEnumerable<purchaseorder>, IEnumerable<PurchaseOrderViewModel>>(orders);
        }

        public IEnumerable<PurchaseOrderViewModel> GetPurchaseOrdersContainsBy(string searchStr)
        {
            var orders = _context.purchaseorders.OrderByDescending(x => x.Created).AsQueryable();

            // return all orders if the search string is empty
            if (string.IsNullOrWhiteSpace(searchStr))
                return Mapper.Map<IEnumerable<purchaseorder>, IEnumerable<PurchaseOrderViewModel>>(orders);

            var resultOrder = orders.Where(x => x.Id.Contains(searchStr)
                || x.vendor.Name.Contains(searchStr));
                //|| x.Status.Contains(searchStr));

            return Mapper.Map<IEnumerable<purchaseorder>, IEnumerable<PurchaseOrderViewModel>>(resultOrder);
        }

        public PurchaseOrderViewModel GetPurchaseOrder(string id)
        {
            var order = _context.purchaseorders.FirstOrDefault(x => x.Id == id);

            return Mapper.Map<purchaseorder, PurchaseOrderViewModel>(order);
        }

        public IEnumerable<PurchaseOrderItem> GetPurchaseOrderItems(string poId)
        {
            var orderItems = _context.purchaseorderitems
                .Where(x => x.PurchaseOrderId == poId)
                .OrderBy(x => x.Id);

            return Mapper.Map<IEnumerable<purchaseorderitem>, IEnumerable<PurchaseOrderItem>>(orderItems);
        }

        public PurchaseOrderViewModel UpdatePurchaseOrderItems(string poId, List<long> paidPoItems ,List<long> unpaidPoItems)
        {
            // get the po 
            var po = _context.purchaseorders.FirstOrDefault(x => x.Id == poId);

            // update the PO items as paid if their ID exists in the list
            if (paidPoItems != null)
            {
                var tobePaidItems = po.purchaseorderitems.Where(x => paidPoItems.Contains(x.Id));
                foreach (var item in tobePaidItems)
                    item.IsPaid = true;
            }

            // update the items for not yet paid
            if (unpaidPoItems != null)
            {
                var notYetPaidItems = po.purchaseorderitems.Where(x => unpaidPoItems.Contains(x.Id));
                foreach (var item in notYetPaidItems)
                    item.IsPaid = false;
            }

            var itemPaidStatus = po.purchaseorderitems.Select(x => x.IsPaid).Distinct().ToList();
            po.PaymentStatus = itemPaidStatus.Count() == 2 
                ? PaymentStatus.PartiallyPaid : (itemPaidStatus.First() ? PaymentStatus.Paid : PaymentStatus.NotPaid);
            po.Modified = DateTime.UtcNow;

            // save the changes
            _context.SaveChanges();

            return Mapper.Map<purchaseorder, PurchaseOrderViewModel>(po);

        }

        public PurchaseOrderViewModel SavePurchaseOrder(PurchaseOrderViewModel model)
        {
            var po = Mapper.Map<PurchaseOrderViewModel, purchaseorder>(model);
            po.Modified = DateTime.UtcNow;
            po.Created = DateTime.UtcNow;

            _context.purchaseorders.Add(po);
            _context.SaveChanges();

            return Mapper.Map<purchaseorder, PurchaseOrderViewModel>(po);
        }

        public PurchaseOrderViewModel UpdatePurchaseOrderItems(string poId, PurchaseOrderViewModel model)
        {
            var existingPo = _context.purchaseorders.FirstOrDefault(x => x.Id == poId);
            if (existingPo == null)
                return model;

            var updatedPo = Mapper.Map<PurchaseOrderViewModel, purchaseorder>(model);
            var updatedPoItems = updatedPo.purchaseorderitems;
            var existingPoItems = existingPo.purchaseorderitems;

            // let's retain the date when the PO was created
            updatedPo.Created = existingPo.Created;
            updatedPo.Modified = DateTime.UtcNow;

            // find the newly added purchase order items (purchase order items came from client - existing purchase order items = new added purchase order items)
            var addedPoItems = updatedPoItems.Except(existingPoItems, (o1, o2) => (o1.PurchaseOrderId == o2.PurchaseOrderId && o1.SKU == o2.SKU));

            // find deleted purchase order items by exising purchase order items - updated purchase order items = deleted purchase order items
            var deletedPoItems = existingPoItems.Except(updatedPoItems, (o1, o2) => (o1.PurchaseOrderId == o2.PurchaseOrderId && o1.SKU == o2.SKU));

            // find modified purchase order items by updated purchase order items - added orderitems = modified purchase order items
            var modifiedPoItems = updatedPoItems.Except(addedPoItems, (o1, o2) => (o1.PurchaseOrderId == o2.PurchaseOrderId && o1.SKU == o2.SKU)).ToList();

            // apply the modified purchase order items to the current property value 
            foreach (var item in modifiedPoItems)
            {
                var existingPoItem = _context.purchaseorderitems.FirstOrDefault(x => x.PurchaseOrderId == item.PurchaseOrderId && x.SKU == item.SKU);

                // get DBEntity object for the existing purchase order item entity
                var poItemEntry = _context.Entry(existingPoItem);
                poItemEntry.CurrentValues.SetValues(item);
            }

            // mark all added orderitemss entity state to Added
            addedPoItems.ToList().ForEach(item => _context.Entry(item).State = EntityState.Added);

            // mark all deleted order items entity state to Deleted
            deletedPoItems.ToList().ForEach(item => _context.Entry(item).State = EntityState.Deleted);

            // get DBEnity object for the existing purchase order entity
            var poEntry = _context.Entry(existingPo);
            poEntry.CurrentValues.SetValues(updatedPo);

            // let's save the changes
            _context.SaveChanges();

            return model;
        }

        public void DeleteBillings(bool isSelectAllPages, List<string> billingIds)
        {
            // get the billings to delete
            var billings = _context.purchaseorders
                .Where(x => (isSelectAllPages == true && !billingIds.Contains(x.Id)) || billingIds.Contains(x.Id));

            // delete first the po items
            _context.purchaseorderitems.RemoveRange(billings.SelectMany(x => x.purchaseorderitems));

            // then the parent item
            _context.purchaseorders.RemoveRange(billings);

            // save the changes
            _context.SaveChanges();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    _context.Dispose();
                    // Dispose other managed resources.
                }
                //release unmanaged resources.
            }
            _disposed = true;
        }
    }
}
