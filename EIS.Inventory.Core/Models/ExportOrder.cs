using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Core.Models
{
    public class ExportOrder
    {
        public string OrderFields { get; set; }
        public string SelectedEisOrderId { get; set; }
        public string ExcludedEisOrderId { get; set; }
        public string Delimiter { get; set; }
        public string SortBy { get; set; }
        public bool IsAllOderItems { get; set; }
        public DateTime RequestedDate { get; set; }
        public List<string> SelectedEisOrderIdArr
        {
            get
            {
                return string.IsNullOrEmpty(SelectedEisOrderId) ? new List<string>() : SelectedEisOrderId.Split(',').ToList();
            }
        }
        public List<string> ExcludedEisOrderIdArr
        {
            get
            {
                return string.IsNullOrEmpty(ExcludedEisOrderId) ? new List<string>() : ExcludedEisOrderId.Split(',').ToList();
            }
        }
        public List<string> OrderFieldsArr
        {
            get
            {
                if (string.IsNullOrEmpty(OrderFields))
                {
                    return new List<string>();
                }

                var fieldList = OrderFields.Split(',').ToList();
                return fieldList;
            }
        }

    }
}
