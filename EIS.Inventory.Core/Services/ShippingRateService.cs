using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using System;

namespace EIS.Inventory.Core.Services {
    public class ShippingRateService : IShippingRateService, IDisposable
    {
        private bool _disposed;
        private readonly EisInventoryContext _context;

        public ShippingRateService ()
        {
            _context = new EisInventoryContext();
        }
        
        public IEnumerable<ShippingRateDto> GetAllShippingRates ()
        {
            var shippingrates = _context.shippingrates.OrderBy(x => x.WeightFrom).ToList();

            return Mapper.Map<IEnumerable<shippingrate>, IEnumerable<ShippingRateDto>>(shippingrates);
        }

        public ShippingRateDto GetShippingRate ( int shippingRateId )
        {
            var shippingrate = _context.shippingrates.SingleOrDefault(x => x.Id == shippingRateId);
            return Mapper.Map<shippingrate, ShippingRateDto>(shippingrate);
        }

        public ShippingRateDto CreateShippingRate ( ShippingRateDto model )
        {
            var shippingrate = Mapper.Map<ShippingRateDto, shippingrate>(model);

            _context.shippingrates.Add(shippingrate);
            _context.SaveChanges();

            return Mapper.Map<shippingrate, ShippingRateDto>(shippingrate);
        }

        public ShippingRateDto UpdateShippingRatey ( ShippingRateDto model )
        {
            var oldShippingRate = _context.shippingrates.FirstOrDefault(x => x.Id == model.Id);
            var shippingrate = Mapper.Map<ShippingRateDto, shippingrate>(model);

            _context.Entry(oldShippingRate).CurrentValues.SetValues(shippingrate);
            _context.SaveChanges();

            return model;
        }

        public bool DeleteShippingRate ( int shippingRateId )
        {
            var shippingrate = _context.shippingrates.SingleOrDefault(x => x.Id == shippingRateId);
            if (shippingrate == null)
                return true;

            _context.shippingrates.Remove(shippingrate);
            _context.SaveChanges();

            return true;
        }

        #region IDisposable
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
        #endregion
    }
}
