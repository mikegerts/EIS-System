using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.DAL.Database
{
    public partial class customexportproducttask
    {
        public List<int> CompanyIdsList
        {
            get
            {
                return string.IsNullOrEmpty(CompanyIds)
                    ? null 
                    : CompanyIds.Split(',').Select(int.Parse).ToList();
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
                return string.IsNullOrEmpty(CustomFields)
                    ? new List<string>()
                    : CustomFields.Split(',').ToList();
            }
        }

        public int CustomFieldsCount
        {
            get { return CustomFieldsList.Count(); }
        }
    }
}
