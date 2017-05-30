using EIS.Inventory.Core.Models;
using EIS.Inventory.Shared.Models;
using System;

namespace EIS.Inventory.Core.ViewModels
{
    public class LogViewModel
    {
        public int Id { get; set; }

        public LogEntrySeverity Severity { get; set; }

        public LogEntryType EntryType { get; set; }

        public string EntryTypeStr { get; set; }

        public string Description { get; set; }

        public string StackTrace { get; set; }

        public DateTime Created { get; set; }
    }
}
