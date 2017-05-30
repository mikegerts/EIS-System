using System.ComponentModel;

namespace AmazonWebServiceModels
{
    /// <summary>
    /// This defines the feed types for the Amazon MWS
    /// </summary>
    public enum AmazonFeedType
    {
        /// <summary>
        /// add products to listing
        /// </summary>
        [Description("Product Feed")]
        _POST_PRODUCT_DATA_,

        /// <summary>
        /// for item relationship feed
        /// </summary>
        [Description("Relationships Feed")]
        _POST_PRODUCT_RELATIONSHIP_DATA_,

        /// <summary>
        /// for images feed type
        /// </summary>
        [Description("Product Images Feed")]
        _POST_PRODUCT_IMAGE_DATA_,

        /// <summary>
        /// update eisProduct pricing
        /// </summary>
        [Description("Pricing Feed")]
        _POST_PRODUCT_PRICING_DATA_,

        /// <summary>
        /// override info, such as shipping costs
        /// </summary>
        [Description("Shipping Override Feed")]
        _POST_PRODUCT_OVERRIDES_DATA_,

        /// <summary>
        /// update inventory eisProduct's quantity
        /// </summary>
        [Description("Inventory Feed")]
        _POST_INVENTORY_AVAILABILITY_DATA_,

        /// <summary>
        /// for eisProduct shipment confirmation type
        /// </summary>
        [Description("Order Fulfillment Feed")]
        _POST_ORDER_FULFILLMENT_DATA_,
    }
}
