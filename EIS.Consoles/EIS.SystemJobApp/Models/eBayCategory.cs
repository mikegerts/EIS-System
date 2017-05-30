
namespace EIS.SystemJobApp.Models
{
    public class eBayCategory
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string OptionName
        {
            get { return string.Format("{0} - {1}", Id, Name); }
        }
    }
}
