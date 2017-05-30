using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models.Shippings
{
    public class EisLabelSpecification
    {
        public EisShippingDocumentImageType ImageType { get; set; }
        public bool ImageTypeSpecified { get; set; }
        public EisLabelFormatType LabelFormatType { get; set; }
        public EisLabelStockType LabelStockType { get; set; }
        public bool LabelStockTypeSpecified { get; set; }
        public EisLabelPrintingOrientationType LabelPrintingOrientation{ get; set; }
        public bool LabelPrintingOrientationSpecified { get; set; }

    }

    public enum EisShippingDocumentImageType
    {
        DOC, EPL2, PDF, PNG, RTF, TEXT, ZPLII
    }

    public enum EisLabelFormatType
    {
        COMMON2D, FEDEX_FREIGHT_STRAIGHT_BILL_OF_LADING, LABEL_DATA_ONLY, VICS_BILL_OF_LADING
    }

    public enum EisLabelStockType
    {
        PAPER_4X6, PAPER_4X8, PAPER_4X9, PAPER_7X475, PAPER_85X11_BOTTOM_HALF_LABEL, PAPER_85X11_TOP_HALF_LABEL, PAPER_LETTER,
        STOCK_4X6, STOCK_4X675_LEADING_DOC_TAB, STOCK_4X675_TRAILING_DOC_TAB, STOCK_4X8, STOCK_4X9_LEADING_DOC_TAB, STOCK_4X9_TRAILING_DOC_TAB
    }
    public enum EisLabelPrintingOrientationType
    {
        BOTTOM_EDGE_OF_TEXT_FIRST, TOP_EDGE_OF_TEXT_FIRST
    }

}
