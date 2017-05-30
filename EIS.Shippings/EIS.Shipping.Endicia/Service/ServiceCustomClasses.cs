namespace EIS.Shipping.Endicia.Service
{
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlRoot(Namespace = "www.envmgr.com/LabelService")]
    public partial class CalculatePostageRatesResponse : BaseResponse
    {
        [System.Xml.Serialization.XmlElementAttribute("PostageRatesResponse")]
        public PostageRatesResponse PostageRatesResponse { get; set; }
    }
}
