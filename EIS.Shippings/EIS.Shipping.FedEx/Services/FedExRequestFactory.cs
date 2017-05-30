namespace EIS.Shipping.FedEx.Services
{
    public static class FedExRequestFactory
    {
        public enum RequestType
        {
            Shipment, Rate
        }
        public static IFedExRequest CreateRequest(RequestType requestType)
        {
            switch (requestType)
            {
                case RequestType.Shipment:
                    return new ShipServiceWebReference.ProcessShipmentRequest();
                case RequestType.Rate:
                    return new RateServiceWebReference.RateRequest();
                default:
                    return null;
            }
        }
    }
}
