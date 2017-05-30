using System;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.ViewModels;
using AutoMapper;

namespace EIS.Inventory.Core.Services
{
    public class ShippingService : IShippingService
    {
        private readonly ILogService _logger;
        private readonly EisInventoryContext _context;

        public ShippingService(ILogService logger)
        {
            _logger = logger;
            _context = new EisInventoryContext();
        }

        public IPagedList<OrderProductListDto> GetAwaitingShipments(int page, int pageSize)
        {
            return _context.orders
               .Where(x => x.OrderStatus == Shared.Models.OrderStatus.Unshipped
                    && x.orderitems.Any(o => o.orderproducts.Any()))
               .OrderByDescending(x => x.EisOrderId)
               .ToPagedList(page, pageSize)
               .ToMappedPagedList<order, OrderProductListDto>();
        }

        public OrderProductDetailDto GetOrderProductDetailByOrderId(string orderId)
        {
            var result = _context.orders.FirstOrDefault(x => x.OrderId == orderId);

            return Mapper.Map<OrderProductDetailDto>(result);
        }

        public IPagedList<ShippingLocationDto> GetShippingLocations(int page, int pageSize)
        {
            return _context.shippinglocations
               .OrderBy(x => x.Id)
               .ToPagedList(page, pageSize)
               .ToMappedPagedList<shippinglocation, ShippingLocationDto>();
        }

        public List<ShippingLocationDto> GetAllShippingLocations()
        {
            var results = _context.shippinglocations.ToList();

            return Mapper.Map<List<ShippingLocationDto>>(results);
        }

        public ShippingLocationDto GetShippingLocationById(int id)
        {
            var result = _context.shippinglocations.FirstOrDefault(x => x.Id == id);

            return Mapper.Map<ShippingLocationDto>(result);
        }

        public bool CreateShippingLocation(ShippingLocationDto model)
        {
            // unbox the correct object type for the credential
            var location = Mapper.Map<shippinglocation>(model);

            // set the audit logs
            location.CreatedBy = model.ModifiedBy;
            location.Created = DateTime.UtcNow;
            location.FromAddressDetails.CreatedBy = model.ModifiedBy;
            location.FromAddressDetails.Created = DateTime.UtcNow;
            location.ReturnAddressDetails.CreatedBy = model.ModifiedBy;
            location.ReturnAddressDetails.Created = DateTime.UtcNow;

            using (var transaction = _context.Database.BeginTransaction())
            {
                // save the from address first
                var fromAddress = _context.addressdetails.Add(location.FromAddressDetails);
                _context.SaveChanges();

                // then the return address
                var returnAddress = _context.addressdetails.Add(location.ReturnAddressDetails);
                _context.SaveChanges();

                // set the address ids
                location.FromAddressId = fromAddress.Id;
                location.ReturnAddressId = returnAddress.Id;

                _context.shippinglocations.Add(location);
                _context.SaveChanges();
                model.Id = location.Id;

                transaction.Commit();
            }

            // if this shipping location is set to default, mark the others to False
            if (model.IsDefault)
            {
                var locations = _context.shippinglocations.ToList();
                foreach (var item in locations)
                {
                    if (item.Id == model.Id)
                        continue;
                    item.IsDefault = false;
                }
                _context.SaveChanges();
            }

            return true;
        }

        public bool UpdateShippingLocation(int id, ShippingLocationDto model)
        {
            // get the curent credential
            var existingLocation = _context.shippinglocations.FirstOrDefault(x => x.Id == id);
            if (existingLocation == null)
                return false;

            // unbox to the correct object for crendential
            Mapper.Map(model, existingLocation);
            existingLocation.ModifiedBy = model.ModifiedBy;
            existingLocation.Modified = DateTime.UtcNow;
            
            // update the from address
            var fromAddress = _context.addressdetails.FirstOrDefault(x => x.Id == existingLocation.FromAddressId);
            Mapper.Map(model.FromAddressDetails, fromAddress);
            fromAddress.ModifiedBy = model.ModifiedBy;
            fromAddress.Modified = DateTime.UtcNow;

            // then the return address
            var returnAddress = _context.addressdetails.FirstOrDefault(x => x.Id == existingLocation.ReturnAddressId);
            Mapper.Map(model.ReturnAddressDetails, returnAddress);
            returnAddress.ModifiedBy = model.ModifiedBy;
            returnAddress.Modified = DateTime.UtcNow;

            // if this shipping location is set to default, mark the others to False
            if (model.IsDefault)
            {
                var locations = _context.shippinglocations.ToList();
                foreach(var item in locations)
                {
                    if (item.Id == existingLocation.Id)
                        continue;

                    item.IsDefault = false;
                }
            }

            // save all the changess
            _context.SaveChanges();

            return true;
        }

        public void DeleteShippingLocation(int id)
        {
            var location = _context.shippinglocations.FirstOrDefault(x => x.Id == id);
            if (location == null)
                return;

            var fromAddress = _context.addressdetails.FirstOrDefault(x => x.Id == location.FromAddressId);
            _context.addressdetails.Remove(fromAddress);
            
            var returnAddress = _context.addressdetails.FirstOrDefault(x => x.Id == location.ReturnAddressId);
            _context.addressdetails.Remove(returnAddress);

            // lastly, the location
            _context.shippinglocations.Remove(location);

            _context.SaveChanges();
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _context.Dispose();
                _logger.Dispose();
            }
        }
        #endregion
    }
}
