using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using X.PagedList;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Core.Helpers;

namespace EIS.Inventory.Core.Services
{
    public class ProductGroupService : IProductGroupService
    {
        private readonly EisInventoryContext _context;

        public ProductGroupService()
        {
            _context = new EisInventoryContext();
        }

        public IEnumerable<ProductGroupListDto> GetAllProductGroups()
        {
            var groups = _context.productgroupdetails
                .Select(x => new ProductGroupListDto
                {
                    Id = x.Id,
                    Name = x.Name,
                    Description = x.Description
                })
                .ToList();

            return groups;
        }

        public IPagedList<ProductGroupListDto> GetPagedProductGroups(int page, int pageSize)
        {
            return _context.productgroupdetails
                .OrderBy(x => x.Id)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<productgroupdetail, ProductGroupListDto>();
        }

        public ProductGroupDetailDto GetProductGroupDetails(long id, int page = 1, int pageSize = 10)
        {
            var group = _context.productgroupdetails.FirstOrDefault(x => x.Id == id);
            var products = _context.productgroupdetails.Where(x => x.Id == id)
                .SelectMany(x => x.products)
                .OrderBy(x=> x.EisSKU)
                .ToPagedList(page, pageSize)
                .ToMappedPagedList<product, ProductListDto>();

            return new ProductGroupDetailDto
            {
                Id = group.Id,
                Name = group.Name,
                Description = group.Description,
                Products = products
            };
        }

        public IEnumerable<ProductDto> GetProductsByGroup(long groupId)
        {
            var group = _context.productgroupdetails.FirstOrDefault(x => x.Id == groupId);
            if (group == null)
                throw new ArgumentException(string.Format("Product group ID {0} does not exist!", groupId));

            return Mapper.Map<IEnumerable<product>, IEnumerable<ProductDto>>(group.products);
        }

        public ProductGroupDetailDto CreateProductGroup(ProductGroupDetailDto model)
        {
            var groupDetail = new productgroupdetail
            {
                Id = model.Id,
                Name = model.Name,
                Description = model.Description,
                Created = DateTime.UtcNow,
            };

            // add the new products
            foreach (var sku in model.AddedItems)
            {
                var product = _context.products.FirstOrDefault(x => x.EisSKU == sku);
                if (product == null)
                    continue;

                groupDetail.products.Add(product);
            }

            _context.productgroupdetails.Add(groupDetail);
            _context.SaveChanges();
            model.Id = groupDetail.Id;

            return model;
        }

        public ProductGroupDetailDto UpdateProductGroup(long id, ProductGroupDetailDto model)
        {
            var oldGroupDetail = _context.productgroupdetails.FirstOrDefault(x => x.Id == id);
            if (oldGroupDetail == null || id != model.Id)
                throw new ArgumentException(string.Format("Product group ID {0} is not found!", id));
            
            // add the new products
            foreach (var sku in model.AddedItems)
            {
                var product = _context.products.FirstOrDefault(x => x.EisSKU == sku);
                if (product == null)
                    continue;

                oldGroupDetail.products.Add(product);
            }

            // remove the deleted items
            foreach(var sku in model.DeletedItems)
            {
                var product = oldGroupDetail.products.FirstOrDefault(x => x.EisSKU == sku);
                if (product == null)
                    continue;

                oldGroupDetail.products.Remove(product);
            }

            oldGroupDetail.Modified = DateTime.UtcNow;
            _context.Entry(oldGroupDetail).CurrentValues.SetValues(model);
            _context.SaveChanges();

            return model;
        }

        public bool DeleteProductGroup(long id)
        {
            var group = _context.productgroupdetails.FirstOrDefault(x => x.Id == id);
            if (group == null)
                return true;

            // clear its products
            group.products.Clear();
            
            _context.productgroupdetails.Remove(group);
            _context.SaveChanges();

            return true;
        }

        public int UpdateProductGroupEisSKUs(int groupId, List<string> eisSkuRecords)
        {
            var group = _context.productgroupdetails.FirstOrDefault(x => x.Id == groupId);
            if (group == null)
                throw new ArgumentException(string.Format("Product group ID {0} does not exist!", groupId));

            if (!eisSkuRecords.Any())
                return 0;

            // clear the existing EIS SKUs of the product grooup.
            group.products.Clear();

            var counter = 0;
            foreach (var sku in eisSkuRecords)
            {
                // let's check if the EIS SKU exist
                var product = _context.products.FirstOrDefault(x => x.EisSKU == sku);
                if (product == null)
                    continue;

                counter++;
                group.products.Add(product);
            }

            // save the changes
            _context.SaveChanges();

            return counter;
        }

        #region IDisposable
        public void Dispose()
        {
            _context.Dispose();
            GC.SuppressFinalize(this);
            Dispose(true);
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
