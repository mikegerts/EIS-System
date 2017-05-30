using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.DAL.Database
{
   public partial class customimportordertask
    {
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
