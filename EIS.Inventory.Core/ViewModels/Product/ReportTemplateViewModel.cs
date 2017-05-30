namespace EIS.Inventory.Core.ViewModels
{
    public class ReportTemplateViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string FileFormat { get; set; }
        public string SortField { get; set; }
        public string Fields { get; set; }
        public string Discriminator { get; set; }

    }
}

