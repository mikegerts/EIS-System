
namespace AmazonWebServiceModels
{
    /// <summary>
    /// There are limits to how often Amazon will generate FBA reports. 
    /// These limits depend on whether an FBA report is a near real-time report or a daily report. 
    /// See the following table to see which FBA reports are near real-time and which are daily.
    /// 
    /// A near real-time FBA report is generated no more than once every 30 minutes. 
    /// This means that after a near real-time FBA report is generated following your report request,
    /// a 30-minute waiting period must pass before Amazon will generate an updated version of that report. 
    /// Note that the four "All Orders" reports are not subject to this limitation.
    /// 
    /// A daily FBA report is generated no more than once every four hours. 
    /// This means that after a daily FBA report is generated following your report request,
    /// a four-hour waiting period must pass before Amazon will generate an updated version of that report.
    /// </summary>
    public enum FbaReportType
    {
        /**FBA Sales Reports**/

        /// <summary>
        /// Tab-delimited flat file. 
        /// Contains detailed order/shipment/item information including price, address, and tracking data. 
        /// You can request up to one month of data in a single report. 
        /// Content updated near real-time in Europe (EU), Japan, and North America (NA). 
        /// In China, content updated daily. For FBA sellers only. 
        /// </summary>
        _GET_AMAZON_FULFILLED_SHIPMENTS_DATA_,

        /// <summary>
        /// Tab-delimited flat file. Returns all orders updated in the specified date range regardless of fulfillment channel or shipment status. 
        /// This report is intended for order tracking, not to drive your fulfillment process;
        /// it does not include customer identifying information and scheduling is not supported
        /// </summary>
        _GET_FLAT_FILE_ALL_ORDERS_DATA_BY_LAST_UPDATE_,

        /// <summary>
        /// Tab-delimited flat file. Returns all orders placed in the specified date range regardless of fulfillment channel or shipment status.
        /// This report is intended for order tracking, not to drive your fulfillment process; 
        /// it does not include customer identifying information and scheduling is not supported.
        /// </summary>
        _GET_FLAT_FILE_ALL_ORDERS_DATA_BY_ORDER_DATE_,

        /// <summary>
        /// XML file order report that returns all orders updated in the specified date range regardless of fulfillment channel or shipment status.
        /// This report is intended for order tracking, not to drive your fulfillment process;
        /// it does not include customer identifying information and scheduling is not supported.
        /// </summary>
        _GET_XML_ALL_ORDERS_DATA_BY_LAST_UPDATE_,

        /// <summary>
        /// XML file order report that returns all orders placed in the specified date range regardless of fulfillment channel or shipment status. 
        /// This report is intended for order tracking, not to drive your fulfillment process; 
        /// it does not include customer identifying information and scheduling is not supported.
        /// </summary>
        _GET_XML_ALL_ORDERS_DATA_BY_ORDER_DATE_,
        

        /**FBA Inventory Reports**/

        /// <summary>
        /// Tab-delimited flat file. Contains condensed item level data on shipped FBA customer orders including price, quantity, and ship to location. 
        /// Content updated near real-time in Europe (EU), Japan, and North America (NA). In China, content updated daily. 
        /// </summary>
        _GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_SALES_DATA_,

        /// <summary>
        /// ab-delimited flat file. Contains promotions applied to FBA customer orders sold through Amazon; e.g. Super Saver Shipping. Content updated daily.
        /// </summary>
        _GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_PROMOTION_DATA_,
        
        /// <summary>
        /// Tab-delimited flat file for tax-enabled US sellers.
        /// This report contains data through February 28, 2013. 
        /// All new transaction data can be found in the Sales Tax Report. 
        /// </summary>
        _GET_FBA_FULFILLMENT_CUSTOMER_TAXES_DATA_,

        _GET_AFN_INVENTORY_DATA_,

        _GET_AFN_INVENTORY_DATA_BY_COUNTRY_,

        _GET_FBA_FULFILLMENT_CURRENT_INVENTORY_DATA_,

        _GET_FBA_FULFILLMENT_MONTHLY_INVENTORY_DATA_,

        _GET_FBA_FULFILLMENT_INVENTORY_RECEIPTS_DATA_,
        
        _GET_RESERVED_INVENTORY_DATA_,

        _GET_FBA_FULFILLMENT_INVENTORY_SUMMARY_DATA_,

        _GET_FBA_FULFILLMENT_INVENTORY_ADJUSTMENTS_DATA_,

        _GET_FBA_FULFILLMENT_INVENTORY_HEALTH_DATA_,

        _GET_FBA_MYI_UNSUPPRESSED_INVENTORY_DATA_,

        _GET_FBA_MYI_ALL_INVENTORY_DATA_,

        _GET_FBA_FULFILLMENT_CROSS_BORDER_INVENTORY_MOVEMENT_DATA_,

        _GET_FBA_FULFILLMENT_INBOUND_NONCOMPLIANCE_DATA_,

        _GET_FBA_HAZMAT_STATUS_CHANGE_DATA_,


        /**FBA Payments Reports**/

        _GET_FBA_ESTIMATED_FBA_FEES_TXT_DATA_,

        _GET_FBA_REIMBURSEMENTS_DATA_,


        /**FBA Customer Concessions Reports**/

        _GET_FBA_FULFILLMENT_CUSTOMER_RETURNS_DATA_,

        _GET_FBA_FULFILLMENT_CUSTOMER_SHIPMENT_REPLACEMENT_DATA_,


        /**FBA Removals Reports**/

        _GET_FBA_RECOMMENDED_REMOVAL_DATA_,

        _GET_FBA_FULFILLMENT_REMOVAL_ORDER_DETAIL_DATA_,

        _GET_FBA_FULFILLMENT_REMOVAL_SHIPMENT_DETAIL_DATA_,
    }
}
