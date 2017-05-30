
namespace AmazonWebServiceModels
{
    /// <summary>
    /// Note that settlement reports cannot be requested or scheduled. 
    /// They are automatically scheduled by Amazon.
    /// </summary>
    public enum SettlementReportType
    {
        /// <summary>
        /// Tab-delimited flat file settlement report that is automatically scheduled by Amazon; 
        /// it cannot be requested through RequestReport. For all sellers.
        /// </summary>
        _GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_,

        /// <summary>
        /// XML file settlement report that is automatically scheduled by Amazon;
        /// it cannot be requested through RequestReport. For Seller Central sellers only.
        /// </summary>
        _GET_V2_SETTLEMENT_REPORT_DATA_XML_,

        /// <summary>
        /// Tab-delimited flat file alternate version of the Flat File Settlement Report 
        /// that is automatically scheduled by Amazon; it cannot be requested through RequestReport. 
        /// Price columns are condensed into three general purpose columns: amounttype, amountdescription, and amount.
        /// </summary>
        _GET_V2_SETTLEMENT_REPORT_DATA_FLAT_FILE_V2_,
    }
}
