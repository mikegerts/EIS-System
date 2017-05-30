namespace EIS.Shipping.FedEx.RateServiceWebReference
{
    public partial class RateService
    {
        public RateService(string mode)
        {
            if (mode == "LIVE")
                Url = "https://ws.fedex.com:443/web-services/rate";
            else
                Url = "https://wsbeta.fedex.com:443/web-services/rate";
        }
    }
}
