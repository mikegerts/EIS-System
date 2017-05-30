using AutoMapper;
using EIS.Inventory.Core.Mappings;

namespace EIS.Inventory
{
    public class AutoMapperConfig
    {
        public static void RegisterAutoMappers()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<ViewModelToDomainMappingProfile>();
                x.AddProfile<DomainToViewModelMappingProfile>();
            });
        }
    }
}