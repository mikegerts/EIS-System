using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using X.PagedList;

namespace EIS.Inventory.Core.Services
{
    public class KitService : IKitService
    {
        private readonly EisInventoryContext _context;

        public KitService()
        {
            _context = new EisInventoryContext();
        }
        
        public KitDto GetKitByParentKitSku(string parentKitSKU)
        {
            var kit = _context.kits.FirstOrDefault(x => x.ParentKitSKU == parentKitSKU);

            return Mapper.Map<kit, KitDto>(kit);
        }

        public KitDetailDto GetKitDetailByIds(string parentKitSku, string childKitSku)
        {
            var kitDetail = _context.kitdetails
                .FirstOrDefault(x => x.ParentKitSKU == parentKitSku && x.ChildKitSKU == childKitSku);

            return Mapper.Map<kitdetail, KitDetailDto>(kitDetail);
        }

        public IEnumerable<KitDetailDto> GetKitDetailsByParentKitSku(string parentKitSku)
        {
            var kitDetails = _context.kitdetails.Where(x => x.ParentKitSKU == parentKitSku);

            return Mapper.Map<IEnumerable<kitdetail>, IEnumerable<KitDetailDto>>(kitDetails);
        }

        public IPagedList<ProductSearchDto> GetProducts(int page, int pageSize, string searchStr)
        {
            return _context.products.Where(x => x.EisSKU.Contains(searchStr)
                        || x.Name.Contains(searchStr))
                .OrderBy(x => x.EisSKU)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<product, ProductSearchDto>();
        }
        
        public KitDto UpdateKit(string parentKitSku, KitDto model)
        {
            var oldKit = _context.kits.FirstOrDefault(x => x.ParentKitSKU == parentKitSku);

            // create a new Kit if it doesn't exist
            if (oldKit == null)
            {
                var kit = Mapper.Map<KitDto, kit>(model);
                _context.kits.Add(kit);
            }
            else
            {
                //otherwise, update the kit info
                _context.Entry(oldKit).CurrentValues.SetValues(model);
            }

            // update too the product's IsKit
            var product = _context.products.FirstOrDefault(x => x.EisSKU == model.ParentKitSKU);
            if (product != null)
                product.IsKit = model.IsKit;

            // then, save all the changes
            _context.SaveChanges();

            return model;
        }

        public KitDetailDto UpdateKitDetail(KitDetailDto model)
        {
            var oldKitDetail = _context.kitdetails
                .FirstOrDefault(x => x.ParentKitSKU == model.ParentKitSKU && x.ChildKitSKU == model.ChildKitSKU);
            if (oldKitDetail == null)
                return null;

            // upddate the kit detail info
            _context.Entry(oldKitDetail).CurrentValues.SetValues(model);
            _context.SaveChanges();

            return model;
        }

        public KitDto SaveKitDetails(string parentKitSKU, List<KitDetailDto> models)
        {
            var kit = _context.kits.FirstOrDefault(x => x.ParentKitSKU == parentKitSKU);

            // create new if kit is not yet created
            if (kit == null)
            {
                kit = new kit
                {
                    ParentKitSKU = parentKitSKU,
                    InventoryDependencyOn = InventoryDependency.AllComponensts
                };

                // add to the context and save it
                _context.kits.Add(kit);
                _context.SaveChanges();
            }

            var kitDetails = Mapper.Map<List<KitDetailDto>, List<kitdetail>>(models);
            
            // get the unique newly added items
            var newlyAddedItems = kitDetails.Except(kit.kitdetails.ToList(), (o1, o2) => (o1.ChildKitSKU == o2.ChildKitSKU));
            if(!newlyAddedItems.Any())
                return Mapper.Map<kit, KitDto>(kit);

            _context.kitdetails.AddRange(newlyAddedItems);
            _context.SaveChanges();

            // update the product's IsKit
            var product = _context.products.FirstOrDefault(x => x.EisSKU == parentKitSKU);
            if (product != null)
            {
                product.IsKit = true;

                // save the change
                _context.SaveChanges();
            }

            return Mapper.Map<kit, KitDto>(kit);
        }

        public bool DeleteKitDetail(string parentKitSku, string childKitSku)
        {
            var kitDetail = _context.kitdetails
                .FirstOrDefault(x => x.ParentKitSKU == parentKitSku && x.ChildKitSKU == childKitSku);
            if (kitDetail == null)
                return true;

            _context.kitdetails.Remove(kitDetail);
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
            if (disposing)
            {
                _context.Dispose();
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }
        #endregion
    }
}
