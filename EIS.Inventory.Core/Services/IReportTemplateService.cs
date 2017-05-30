using System;
using System.Collections.Generic;
using EIS.Inventory.Core.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public interface IReportTemplateService : IDisposable
    {
        ReportTemplateViewModel SaveTemplate(ReportTemplateViewModel model);
        void DeleteTemplate(int id);
        ReportTemplateViewModel GetReportTemplateById(int id);
        IEnumerable<ReportTemplateViewModel> GetReportTemplates();
    }
}
