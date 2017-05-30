
namespace AmazonWebServiceModels
{
    /// <summary>
    /// These order tracking reports are available in North America (NA) and Europe (EU),
    /// and can be used by all Amazon sellers.
    /// These reports return all orders, regardless of fulfillment channel or shipment status. 
    /// These reports are intended for order tracking, not to drive your fulfillment process,
    /// as the reports do not include customer-identifying information and scheduling is not supported. 
    /// Also note that for MFN orders, item price is not shown for orders in a "pending" state.
    /// </summary>
    public enum OrderTrackingReportType
    {
        /// <summary>
        /// Tab-delimited flat file report that shows all orders updated in the specified period.
        /// Cannot be scheduled. For all sellers.
        /// </summary>
        _GET_FLAT_FILE_ALL_ORDERS_DATA_BY_LAST_UPDATE_,

        /// <summary>
        /// Tab-delimited flat file report that shows all orders that were placed in the specified period.
        /// Cannot be scheduled. For all sellers.
        /// </summary>
        _GET_FLAT_FILE_ALL_ORDERS_DATA_BY_ORDER_DATE_,

        /// <summary>
        /// XML report that shows all orders updated in the specified period.
        /// Cannot be scheduled. For all sellers.
        /// </summary>
        _GET_XML_ALL_ORDERS_DATA_BY_LAST_UPDATE_,

        /// <summary>
        /// XML report that shows all orders that were placed in the specified period. 
        /// Cannot be scheduled. For all sellers.
        /// </summary>
        _GET_XML_ALL_ORDERS_DATA_BY_ORDER_DATE_,
    }
}
