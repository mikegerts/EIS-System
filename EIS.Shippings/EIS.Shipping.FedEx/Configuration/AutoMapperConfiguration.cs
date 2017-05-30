using AutoMapper;

namespace EIS.Shipping.FedEx.Configuration
{
    public class AutoMapperConfiguration
    {
        public static void Configure()
        {
            Mapper.Initialize(x =>
            {
                x.AddProfile<EisShipServiceProfile>();
                x.AddProfile<EisRateServiceProfile>();
            });
        }
    }
}
