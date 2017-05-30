using AutoMapper;
using EIS.Inventory.Core.Models;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Mappings
{
    public class ViewModelToDomainMappingProfile : Profile
    {
        public ViewModelToDomainMappingProfile()
        {
            CreateMap<ProductDto, product>()
                .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model_))
                .ForMember(dest => dest.shadows, opt => opt.Ignore()); // ignore the mapping for shadows inorder to avoid the deletion
            CreateMap<ProductAmazonDto, productamazon>();
            CreateMap<ProducteBayDto, productebay>();
            CreateMap<CountriesViewModel, country>();
            CreateMap<RegionViewModel, region>();
            CreateMap<VendorDto, vendor>();
            CreateMap<ProductTypeViewModel, producttype>();
            CreateMap<CategoryViewModel, amazoncategory>();
            CreateMap<CategoryViewModel, amazonsubcategory>();

            CreateMap<CredentialDto, credential>();
            CreateMap<AmazonCredentialDto, amazoncredential>();
            CreateMap<eBayCredentialDto, ebaycredential>();
            CreateMap<ShipStationCredentialDto, shipstationcredential>();
            CreateMap<Address, addressdetail>();
            CreateMap<ShippingLocationDto, shippinglocation>()
                .ForMember(dest => dest.FromAddressDetails, opt => opt.MapFrom(src => src.FromAddressDetails))
                .ForMember(dest => dest.ReturnAddressDetails, opt => opt.MapFrom(src => src.ReturnAddressDetails));
            CreateMap<OrderItemViewModel, MarketplaceOrderFulfillmentItem>()
                .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.QtyUnshipped));
            CreateMap<OrderShipmentViewModel, MarketplaceOrderFulfillment>()
                .ForMember(dest => dest.FulfillmentDate, opt => opt.MapFrom(src => src.LastUpdateDate));
            CreateMap<MarketplaceRequestReport, requestreport>();
            CreateMap<MarketplaceProcessingReportResult, processingreportresult>();
            CreateMap<MarketplaceProcessingReport, processingreport>();
            CreateMap<OrderShipmentViewModel, ordershipment>();
            CreateMap<OrderItemViewModel, orderitem>()
                .ForMember(dest => dest.ItemId, opt => opt.MapFrom(src => src.MarketplaceItemId));
            CreateMap<OrderProductDto, orderproduct>();
            CreateMap<MarketplaceOrderItem, orderitem>()
                .ForMember(dest => dest.ItemId, opt => opt.MapFrom(src => src.MarketplaceItemId));
            CreateMap<OrderViewModel, order>();
            CreateMap<OrderGroupViewModel, ordergroupdetail>();
            CreateMap<MarketplaceOrder, order>();
            CreateMap<PurchaseOrderItem, purchaseorderitem>();
            CreateMap<PurchaseOrderViewModel, purchaseorder>()
                .ForMember(dest => dest.purchaseorderitems, opt => opt.MapFrom(src => src.Items));
            CreateMap<KitDto, kit>();
            CreateMap<KitDetailDto, kitdetail>();
            CreateMap<CompanyDto, company>()
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault == 1));
            CreateMap<ProductBigCommerceDto, productbigcommerce>();
            CreateMap<BigCommerceCredentialDto, bigcommercecredential>();
            CreateMap<BigCommerceCategoryDto, bigcommercecategory>();
            CreateMap<BigCommerceBrandDto, bigcommercebrand>();
            CreateMap<BigCommerceCustomFieldDto, bigcommercecustomfield>();
            CreateMap<DeparmentDto, department>();
            CreateMap<VendorDepartmentDto, vendordepartment>();
            CreateMap<ReportTemplateViewModel, reporttemplate>();
            CreateMap<ShippingRateDto, shippingrate>();
            CreateMap<MessageTemplateDto, messagetemplate>();
            CreateMap<ScheduledTaskDto, scheduledtask>();
            CreateMap<CustomExportOrderTaskDto, customexportordertask>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? string.Empty : string.Join(",", src.Days)))
                .ForMember(dest => dest.FileHeaders, opt => opt.MapFrom(src => src.FileHeaders == null ? string.Empty : string.Join(",", src.FileHeaders)))
                .ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields == null ? string.Empty : string.Join(",", src.CustomFields)));
            CreateMap<VendorProductFileInventoryTaskDto, vendorproductfileinventorytask>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? string.Empty : string.Join(",", src.Days)));
            CreateMap<GeneratePoTaskDto, generatepotask>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? string.Empty : string.Join(",", src.Days)));
            CreateMap<CustomExportProductTaskDto, customexportproducttask>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? string.Empty : string.Join(",", src.Days)))
                .ForMember(dest => dest.CompanyIds, opt => opt.MapFrom(src => src.CompanyIds == null ? string.Empty : string.Join(",", src.CompanyIds)))
                .ForMember(dest => dest.FileHeaders, opt => opt.MapFrom(src => src.FileHeaders == null ? string.Empty : string.Join(",", src.FileHeaders)))
                .ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields == null ? string.Empty : string.Join(",", src.CustomFields)));
            CreateMap<MarketplaceInventoryTaskDto, marketplaceinventorytask>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? string.Empty : string.Join(",", src.Days)))
                .ForMember(dest => dest.Marketplaces, opt => opt.MapFrom(src => src.Marketplaces == null ? string.Empty : string.Join(",", src.Marketplaces)));
            CreateMap<VendorProductDto, vendorproduct>();
            CreateMap<SystemJobDto, systemjob>();
            CreateMap<SystemEmailDto, systememail>();
            CreateMap<SavedSearchFilterDto, savedsearchfilter>();
            CreateMap<CustomerDto, customer>();
            CreateMap<CustomerNotesDto, customersnote>();
            CreateMap<CustomerAddressDto, customersaddress>();
            CreateMap<CustomerScheduledTaskDto, customerscheduledtask>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? string.Empty : string.Join(",", src.Days)))
                .ForMember(dest => dest.FileHeaders, opt => opt.MapFrom(src => src.FileHeaders == null ? string.Empty : string.Join(",", src.FileHeaders)))
                .ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields == null ? string.Empty : string.Join(",", src.CustomFields)));

            CreateMap<CustomImportOrderTaskDto, customimportordertask>()
                .ForMember(dest => dest.Days, opt => opt.MapFrom(src => src.Days == null ? string.Empty : string.Join(",", src.Days)))
                .ForMember(dest => dest.FileHeaders, opt => opt.MapFrom(src => src.FileHeaders == null ? string.Empty : string.Join(",", src.FileHeaders)))
                .ForMember(dest => dest.CustomFields, opt => opt.MapFrom(src => src.CustomFields == null ? string.Empty : string.Join(",", src.CustomFields)));
        }

        public override string ProfileName
        {
            get { return "DtoToDomainMappings"; }
        }
    }
}
