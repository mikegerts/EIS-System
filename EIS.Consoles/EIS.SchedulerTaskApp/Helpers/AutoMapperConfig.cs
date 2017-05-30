using AutoMapper;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp.Helpers
{
    public static class AutoMapperConfig
    {
        public static void CreateMappings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<scheduledtask, ScheduledTask>();
                cfg.CreateMap<credential, CredentialDto>();
                cfg.CreateMap<amazoncredential, AmazonCredentialDto>();
                cfg.CreateMap<ebaycredential, eBayCredentialDto>()
                .ForMember(dest => dest.eBayDescriptionTemplate, opt => opt.MapFrom(src => src.DescriptionTemplate));
                cfg.CreateMap<shipstationcredential, ShipStationCredentialDto>();
                cfg.CreateMap<productamazon, AmazonPriceFeed>()
                 .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.EisSKU))
                 .ForMember(dest => dest.StandardPrice, opt => opt.MapFrom(src => src.Price));
                cfg.CreateMap<productebay, eBayInventoryFeed>()
                    .ForMember(dest => dest.ProductQuantity, opt => opt.MapFrom(src => src.product.Quantity))
                    .ForMember(dest => dest.IsAlwaysInStock, opt => opt.MapFrom(src => src.product.IsAlwaysInStock))
                    .ForMember(dest => dest.AlwaysQuantity, opt => opt.MapFrom(src => src.product.AlwaysQuantity));
                cfg.CreateMap<product, MarketplacePriceFeedDto>()
                    .ForMember(dest => dest.AmazonPriceFeed, opt => opt.MapFrom(src => src.productamazon))
                    .ForMember(dest => dest.eBayInventoryFeed, opt => opt.MapFrom(src => src.productebay))
                    .ForMember(dest => dest.BigCommerceProductFeed, opt => opt.MapFrom(src => src.productbigcommerce));
                cfg.CreateMap<productamazon, AmazonInventoryFeed>()
                    .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.EisSKU))
                    .ForMember(dest => dest.ProductQuantity, opt => opt.MapFrom(src => src.product.Quantity))
                    .ForMember(dest => dest.IsAlwaysInStock, opt => opt.MapFrom(src => src.product.IsAlwaysInStock))
                    .ForMember(dest => dest.AlwaysQuantity, opt => opt.MapFrom(src => src.product.AlwaysQuantity));
                cfg.CreateMap<productbigcommerce, BigCommerceInventoryFeed>()
                    .ForMember(dest => dest.ProductQuantity, opt => opt.MapFrom(src => src.product.Quantity))
                    .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.EisSKU));
                cfg.CreateMap<product, MarketplaceInventoryFeed>()
                    .ForMember(dest => dest.AmazonInventoryFeed, opt => opt.MapFrom(src => src.productamazon))
                    .ForMember(dest => dest.eBayInventoryFeed, opt => opt.MapFrom(src => src.productebay))
                    .ForMember(dest => dest.BigCommerceInventoryFeed, opt => opt.MapFrom(src => src.productbigcommerce));

                cfg.CreateMap<scheduletaskimportfile, ScheduleTaskImportFiles>();
            });
        }
    }
}
