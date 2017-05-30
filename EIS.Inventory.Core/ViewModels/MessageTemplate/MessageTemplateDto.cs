using EIS.Inventory.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace EIS.Inventory.Core.ViewModels
{
    public class MessageTemplateDto
    {
        public int Id { get; set; }
        public MessageType MessageType { get; set; }
        public string Subject { get; set; }
        public string Description { get; set; }
        public string ContentHtml { get; set; }
        public bool IsEnabled { get; set; }
        public string UserName { get; set; }
        [Required]
        public int SystemEmailId { get; set; }
    }
}
