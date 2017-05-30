
namespace AmazonWebServiceModels
{
    /// <summary>
    /// Note that the date range that you specify when requesting an order report indicates 
    /// when the orders became eligible for fulfillment (no longer in a "pending" state), 
    /// not when the orders were created.
    /// </summary>
    public enum OrderReportType
    {
        /// <summary>
        /// Tab-delimited flat file report that contains only orders that are not confirmed as shipped. 
        /// Can be requested or scheduled. For Marketplace and Seller Central sellers.
        /// </summary>
        _GET_FLAT_FILE_ACTIONABLE_ORDER_DATA_,

        /// <summary>
        /// Scheduled XML order report. For Seller Central sellers only.
        /// </summary>
        _GET_ORDERS_DATA_,

        /// <summary>
        /// Tab-delimited flat file order report that can be requested or scheduled.
        /// The report shows orders from the previous 60 days. 
        /// For Marketplace and Seller Central sellers.
        /// </summary>
        _GET_FLAT_FILE_ORDERS_DATA_,

        /// <summary>
        /// Tab-delimited flat file order report that can be requested or scheduled. 
        /// For Marketplace sellers only.
        /// </summary>
        _GET_CONVERGED_FLAT_FILE_ORDER_REPORT_DATA_,
    }
}
