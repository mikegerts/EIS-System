using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Core.ViewModels
{
    public class MessageTemplateListDto
    {
        public int Id { get; set; }
        public MessageType MessageType { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public bool IsEnabled { get; set; }
        public int SystemEmailId { get; set; }
        public string EmailAddress { get; set; }
    }
}
