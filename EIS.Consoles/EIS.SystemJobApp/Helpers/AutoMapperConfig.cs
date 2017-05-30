using AutoMapper;
using EIS.Inventory.Core.ViewModels;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using EIS.SystemJobApp.Models;

namespace EIS.SystemJobApp.Helpers
{
    public static class AutoMapperConfig
    {
        public static void CreateMappings()
        {
            Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<ProductBigCommerceDto, productbigcommerce>();
                cfg.CreateMap<ProducteBayDto, productebay>();
                cfg.CreateMap<ProductAmazon, productamazon>();
                cfg.CreateMap<systemjob, SystemJob>();
                cfg.CreateMap<credential, CredentialDto>();
                cfg.CreateMap<amazoncredential, AmazonCredentialDto>();
                cfg.CreateMap<ebaycredential, eBayCredentialDto>()
                    .ForMember(dest => dest.eBayDescriptionTemplate, opt => opt.MapFrom(src => src.DescriptionTemplate));
                cfg.CreateMap<shipstationcredential, ShipStationCredentialDto>();
                cfg.CreateMap<productamazon, ProductAmazon>();
                cfg.CreateMap<productamazon, ProductAmazonDto>();
                cfg.CreateMap<productebay, ProducteBayDto>();
                cfg.CreateMap<productamazon, AmazonProductFeed>();
                cfg.CreateMap<productebay, eBayProductFeed>();
                cfg.CreateMap<productbigcommerce, BigCommerceProductFeed>();
                cfg.CreateMap<product, MarketplaceProductFeedDto>()
                    .ForMember(dest => dest.AmazonProductFeed, opt => opt.MapFrom(src => src.productamazon))
                    .ForMember(dest => dest.eBayProductFeed, opt => opt.MapFrom(src => src.productebay))
                    .ForMember(dest => dest.eBayInventoryFeed, opt => opt.MapFrom(src => src.productebay))
                    .ForMember(dest => dest.BigCommerceProductFeed, opt => opt.MapFrom(src => src.productbigcommerce));
                cfg.CreateMap<producttype, ProductTypeViewModel>()
                .ForMember(dest => dest.AmazonMainCategoryName,
                    opt => opt.MapFrom(src => src.amazoncategory.Name))
                .ForMember(dest => dest.AmazonMainClassName,
                    opt => opt.MapFrom(src => src.amazoncategory.ClassName))
                .ForMember(dest => dest.AmazonSubCategoryName,
                    opt => opt.MapFrom(src => src.amazonsubcategory == null ? string.Empty : src.amazonsubcategory.Name))
                .ForMember(dest => dest.AmazonSubClassName,
                    opt => opt.MapFrom(src => src.amazonsubcategory == null ? string.Empty : src.amazonsubcategory.ClassName));
                cfg.CreateMap<product, Inventory.Core.ViewModels.eBayCategoryFeed>()
                .ForMember(dest => dest.AmazonTitle,
                    opt => opt.MapFrom(src => src.productamazon == null ? null : src.productamazon.ProductTitle));
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
                cfg.CreateMap<product, MarketplaceInventoryFeed>()
                    .ForMember(dest => dest.AmazonInventoryFeed, opt => opt.MapFrom(src => src.productamazon))
                    .ForMember(dest => dest.eBayInventoryFeed, opt => opt.MapFrom(src => src.productebay))
                    .ForMember(dest => dest.BigCommerceInventoryFeed, opt => opt.MapFrom(src => src.productbigcommerce));
                cfg.CreateMap<product, ItemFeed>()
                    .ForMember(dest => dest.ItemId, (IMemberConfigurationExpression<product, ItemFeed, string> opt) => opt.MapFrom(src => src.productebay == null ? null : src.productebay.ItemId));
            });
        }
    }
}
