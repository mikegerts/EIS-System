using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.DAL.Database
{
    public partial class exportordertask
    {
        public int IsExported
        {
            get
            {
                if (string.IsNullOrEmpty(ExportMarkIn))
                    return -1;

                return ExportMarkIn.Equals("Exported") ? 1 : 0;
            }
        }

        public List<int> OrderStatusInList
        {
            get
            {
                return string.IsNullOrEmpty(StatusIn)
                    ? new List<int> { -1 }
                    : StatusIn.Split('|').Select(int.Parse).ToList();
            }
        }

        public List<string> OrderFieldsList
        {
            get
            {
                return string.IsNullOrEmpty(OrderFields) ? null : OrderFields.Split(',').ToList();
            }
        }
    }
}
