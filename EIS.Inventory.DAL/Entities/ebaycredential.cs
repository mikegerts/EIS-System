
namespace EIS.Inventory.DAL.Database
{
    public partial class ebaycredential
    {
        public string DescriptionTemplate
        {
            get { return messagetemplate == null ? string.Empty : messagetemplate.ContentHtml; }
        }    
    }
}
