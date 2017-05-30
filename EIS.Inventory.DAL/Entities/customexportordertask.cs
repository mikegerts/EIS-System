using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.DAL.Database
{
    public partial class customexportordertask
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
        public List<string> FileHeadersList
        {
            get
            {
                return string.IsNullOrEmpty(FileHeaders)
                    ? null
                    : FileHeaders.Split(',').ToList();
            }
        }
        public List<string> CustomFieldsList
        {
            get
            {
                return string.IsNullOrEmpty(CustomFields) ? null : CustomFields.Split(',').ToList();
            }
        }
    }
}
