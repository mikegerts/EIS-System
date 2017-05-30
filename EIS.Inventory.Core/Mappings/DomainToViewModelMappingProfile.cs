using System;
using System.Linq;
using AutoMapper;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Mappings
{
    public class DomainToViewModelMappingProfile : Profile
    {
        public DomainToViewModelMappingProfile()
        {
            CreateMap<string, DateTime>().ConvertUsing(new StringToDateTimeConverter());

            CreateMap<country, CountriesViewModel>();
            CreateMap<region, RegionViewModel>();
            CreateMap<vendor, VendorDto>();
            CreateMap<vendor, VendorListDto>();
            CreateMap<producttype, ProductTypeViewModel>()
                .ForMember(dest => dest.AmazonMainCategoryName,
                    opt => opt.MapFrom(src => src.amazoncategory.Name))
                .ForMember(dest => dest.AmazonMainClassName,
                    opt => opt.MapFrom(src => src.amazoncategory.ClassName))
                .ForMember(dest => dest.AmazonSubCategoryName,
                    opt => opt.MapFrom(src => src.amazonsubcategory == null ? string.Empty : src.amazonsubcategory.Name))
                .ForMember(dest => dest.AmazonSubClassName,
                    opt => opt.MapFrom(src => src.amazonsubcategory == null ? string.Empty : src.amazonsubcategory.ClassName));
            CreateMap<productgroupdetail, ProductGroupListDto>();
            CreateMap<amazoncategory, CategoryViewModel>();
            CreateMap<amazonsubcategory, CategoryViewModel>();

            CreateMap<product, ProductSearchDto>();
            CreateMap<product, ProductDto>()
                .ForMember(dest => dest.Model_, opt => opt.MapFrom(src => src.Model));
            CreateMap<product, ProductListDto>();
            CreateMap<product, ProductResultDto>();
            CreateMap<product, eBayCategoryFeed>()
                .ForMember(dest => dest.AmazonTitle,
                    opt => opt.MapFrom(src => src.productamazon == null ? null : src.productamazon.ProductTitle));
            CreateMap<productamazon, AmazonPriceFeed>()
                .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.EisSKU))
                .ForMember(dest => dest.StandardPrice, opt => opt.MapFrom(src => src.Price));
            CreateMap<productebay, eBayInventoryFeed>()
                .ForMember(dest => dest.ProductQuantity, opt => opt.MapFrom(src => src.product.Quantity))
                .ForMember(dest => dest.IsAlwaysInStock, opt => opt.MapFrom(src => src.product.IsAlwaysInStock))
                .ForMember(dest => dest.AlwaysQuantity, opt => opt.MapFrom(src => src.product.AlwaysQuantity));
            CreateMap<product, MarketplacePriceFeedDto>()
                .ForMember(dest => dest.AmazonPriceFeed, opt => opt.MapFrom(src => src.productamazon))
                .ForMember(dest => dest.eBayInventoryFeed, opt => opt.MapFrom(src => src.productebay))
                .ForMember(dest => dest.BigCommerceProductFeed, opt => opt.MapFrom(src => src.productbigcommerce));
            CreateMap<productamazon, AmazonInventoryFeed>()
                .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.EisSKU))
                .ForMember(dest => dest.ProductQuantity, opt => opt.MapFrom(src => src.product.Quantity))
                .ForMember(dest => dest.IsAlwaysInStock, opt => opt.MapFrom(src => src.product.IsAlwaysInStock))
                .ForMember(dest => dest.AlwaysQuantity, opt => opt.MapFrom(src => src.product.AlwaysQuantity));
            CreateMap<product, MarketplaceInventoryFeed>()
                .ForMember(dest => dest.AmazonInventoryFeed, opt => opt.MapFrom(src => src.productamazon))
                .ForMember(dest => dest.eBayInventoryFeed, opt => opt.MapFrom(src => src.productebay))
                .ForMember(dest => dest.BigCommerceInventoryFeed, opt => opt.MapFrom(src => src.productbigcommerce));
            CreateMap<product, ItemFeed>()
                .ForMember(dest => dest.ItemId, (IMemberConfigurationExpression<product, ItemFeed, string> opt) => opt.MapFrom(src => src.productebay == null ? null : src.productebay.ItemId));
            CreateMap<ebaystructuredcategory, MarketplaceCategoryDto>();
            CreateMap<bigcommercecategory, MarketplaceCategoryDto>();
            CreateMap<bigcommercebrand, BigCommerceBrandDto>();
            CreateMap<productbigcommerce, BigCommerceProductFeed>();
            CreateMap<productbigcommerce, ProductBigCommerceDto>();
            CreateMap<productbigcommerce, BigCommerceInventoryFeed>()
                .ForMember(dest => dest.ProductQuantity, opt => opt.MapFrom(src => src.product.Quantity))
                .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.EisSKU));
            CreateMap<productamazon, ProductAmazon>();
            CreateMap<productamazon, ProductAmazonDto>();
            CreateMap<productebay, ProducteBayDto>();
            CreateMap<productamazon, AmazonProductFeed>();
            CreateMap<productebay, eBayProductFeed>();
            CreateMap<product, MarketplaceProductFeedDto>()
                .ForMember(dest => dest.AmazonProductFeed, opt => opt.MapFrom(src => src.productamazon))
                .ForMember(dest => dest.eBayProductFeed, opt => opt.MapFrom(src => src.productebay))
                .ForMember(dest => dest.eBayInventoryFeed, opt => opt.MapFrom(src => src.productebay))
                .ForMember(dest => dest.BigCommerceProductFeed, opt => opt.MapFrom(src => src.productbigcommerce));
            CreateMap<credential, CredentialDto>();
            CreateMap<amazoncredential, AmazonCredentialDto>();
            CreateMap<ebaycredential, eBayCredentialDto>()
                .ForMember(dest => dest.eBayDescriptionTemplate, opt => opt.MapFrom(src => src.DescriptionTemplate));
            CreateMap<shipstationcredential, ShipStationCredentialDto>();
            CreateMap<addressdetail, Address>();
            CreateMap<shippinglocation, ShippingLocationDto>()
                .ForMember(dest => dest.FromAddressDetails, opt => opt.MapFrom(src => src.FromAddressDetails))
                .ForMember(dest => dest.ReturnAddressDetails, opt => opt.MapFrom(src => src.ReturnAddressDetails));
            CreateMap<order, OrderViewModel>();
            CreateMap<ordergroupdetail, OrderGroupViewModel>();
            CreateMap<ordergroupdetail, OrderGroupListViewModel>();
            CreateMap<order, OrderListViewModel>();
            CreateMap<order, OrderProductListDto>()
                .ForMember(dest => dest.ItemSKU, opt => opt.MapFrom(src => src.OrderProductItemSKU))
                .ForMember(dest => dest.ItemName, opt => opt.MapFrom(src => src.OrderProductItemName))
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.OrderProductQuantity))
                .ForMember(dest => dest.BuyerName, opt => opt.MapFrom(src => src.ShippingAddressName));
            CreateMap<order, OrderProductDetailDto>()
                .ForMember(dest => dest.Store, opt => opt.MapFrom(src => src.Marketplace));
            CreateMap<orderproduct, OrderProductDto>();
            CreateMap<orderproduct, OrderProduct>();
            CreateMap<orderitem, OrderItemViewModel>()
                .ForMember(dest => dest.Marketplace, opt => opt.MapFrom(src => src.order.Marketplace))
                .ForMember(dest => dest.MarketplaceItemId, opt => opt.MapFrom(src => src.ItemId));
            CreateMap<orderitem, MarketplaceOrderItem>()
                .ForMember(dest => dest.MarketplaceItemId, opt => opt.MapFrom(src => src.ItemId))
                .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.ItemTax));
            CreateMap<requestreport, RequestReportViewModel>();
            CreateMap<processingreport, MarketplaceProcessingReport>();
            CreateMap<processingreportresult, MarketplaceProcessingReportResult>();
            CreateMap<log, LogViewModel>();
            CreateMap<purchaseorderitem, PurchaseOrderItem>();
            CreateMap<purchaseorder, PurchaseOrderViewModel>()
                .ForMember(dest => dest.Total, opt => opt.MapFrom(src => src.purchaseorderitems.Sum(x => x.Qty * x.UnitPrice)))
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.purchaseorderitems));
            CreateMap<purchaseorder, PurchaseOrderViewModel>()
                .ForMember(dest => dest.ContactPerson, opt => opt.MapFrom(src => src.vendor.ContactPerson))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.vendor.PhoneNumber));
            CreateMap<kit, KitDto>();
            CreateMap<kitdetail, KitDetailDto>();
            CreateMap<shadow, ShadowDto>();
            CreateMap<company, CompanyDto>()
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault ? 1 : 0));
            CreateMap<company, CompanyListDto>();
            CreateMap<bigcommercecredential, BigCommerceCredentialDto>();
            CreateMap<bigcommercecategory, BigCommerceCategoryDto>();
            CreateMap<bigcommercecustomfield, BigCommerceCustomFieldDto>();
            CreateMap<department, DeparmentDto>();
            CreateMap<vendordepartment, VendorDepartmentDto>();
            CreateMap<reporttemplate, ReportTemplateViewModel>();
            CreateMap<shippingrate, ShippingRateDto>();
            CreateMap<messagetemplate, MessageTemplateDto>();
            CreateMap<messagetemplate, MessageTemplateListDto>();
            CreateMap<scheduledtask, ScheduledTaskListDto>();
            CreateMap<customexportordertask, CustomExportOrderTaskDto>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? null : src.Days.Split(',').ToList()))
                .ForMember(dest => dest.FileHeaders, opt => opt.MapFrom(src => src.FileHeaders == null ? null : src.FileHeaders.Split(',').ToList()))
                .ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields == null ? null : src.CustomFields.Split(',').ToList()));
            CreateMap<vendorproductfileinventorytask, VendorProductFileInventoryTaskDto>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? null : src.Days.Split(',').ToList()));
            CreateMap<generatepotask, GeneratePoTaskDto>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? null : src.Days.Split(',').ToList()));
            CreateMap<marketplaceinventorytask, MarketplaceInventoryTaskDto>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? null : src.Days.Split(',').ToList()))
                .ForMember(dest => dest.Marketplaces, opt => opt.MapFrom(src => src.Marketplaces == null ? null : src.Marketplaces.Split(',').ToList()));
            CreateMap<customexportproducttask, CustomExportProductTaskDto>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? null : src.Days.Split(',').ToList()))
                .ForMember(dest => dest.CompanyIds, opt => opt.MapFrom(src => string.IsNullOrEmpty(src.CompanyIds) ? null : src.CompanyIds.Split(',').ToList()))
                .ForMember(dest => dest.FileHeaders, opt => opt.MapFrom(src => src.FileHeaders == null ? null : src.FileHeaders.Split(',').ToList()))
                .ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields == null ? null : src.CustomFields.Split(',').ToList()));
            CreateMap<exportedfile, ExportedFileDto>();
            CreateMap<vendorproduct, VendorProductDto>();
            CreateMap<vendorproduct, VendorProductListDto>();
            CreateMap<vendorproduct, VendorProductResultDto>();
            CreateMap<systemjob, SystemJobDto>();
            CreateMap<systemjob, SystemJobListDto>();
            CreateMap<systememail, SystemEmailDto>();
            CreateMap<systememail, SystemEmailsListDto>();
            CreateMap<savedsearchfilter, SavedSearchFilterDto>();
            CreateMap<savedsearchfilter, SavedSearchFilterListDto>();
            CreateMap<customer, CustomerDto>();
            CreateMap<customer, CustomerListDto>();
            CreateMap<customersnote, CustomerNotesDto>();
            CreateMap<customersnote, CustomerNotesListDto>();
            CreateMap<customersaddress, CustomerAddressDto>();
            CreateMap<customersaddress, CustomerAddressListDto>();
            CreateMap<customerscheduledtask, CustomerScheduledTaskListDto>();
            CreateMap<customerwholesalepricehistory, CustomerWholeSalePriceHistoryDto>();

            CreateMap<customimportordertask, CustomImportOrderTaskDto>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? null : src.Days.Split(',').ToList()))
                .ForMember(dest => dest.FileHeaders, opt => opt.MapFrom(src => src.FileHeaders == null ? null : src.FileHeaders.Split(',').ToList()))
                .ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields == null ? null : src.CustomFields.Split(',').ToList()));

            CreateMap<customerscheduledtask, CustomerScheduledTaskDto>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? null : src.Days.Split(',').ToList()))
                .ForMember(dest => dest.FileHeaders, opt => opt.MapFrom(src => src.FileHeaders == null ? null : src.FileHeaders.Split(',').ToList()))
                .ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields == null ? null : src.CustomFields.Split(',').ToList()));
        }

        public override string ProfileName
        {
            get { return "DomainToDtoMappings"; }
        }
    }
}

