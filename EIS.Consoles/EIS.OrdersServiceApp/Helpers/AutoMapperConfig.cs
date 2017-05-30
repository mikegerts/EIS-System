using AutoMapper;
using BigCommerce4Net.Domain;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.OrdersServiceApp.Helpers
{
    public static class AutoMapperConfig
    {
        public static void CreateMappings()
        {
            // domain to model/viewmodel
            Mapper.Initialize(cfg =>
            {
                // domain to View Model
                cfg.CreateMap<orderproduct, OrderProduct>();
                cfg.CreateMap<orderitem, MarketplaceOrderItem>()
                    .ForMember(dest => dest.MarketplaceItemId, opt => opt.MapFrom(src => src.ItemId))
                    .ForMember(dest => dest.Tax, opt => opt.MapFrom(src => src.ItemTax));
                cfg.CreateMap<credential, CredentialDto>();
                cfg.CreateMap<amazoncredential, AmazonCredentialDto>();
                cfg.CreateMap<ebaycredential, eBayCredentialDto>()
                    .ForMember(dest => dest.eBayDescriptionTemplate, opt => opt.MapFrom(src => src.DescriptionTemplate));
                cfg.CreateMap<shipstationcredential, ShipStationCredentialDto>();
                // ViewModel to domain
                cfg.CreateMap<Category, bigcommercecategory>()
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.ImageFile));
            });
        }
    }
}
