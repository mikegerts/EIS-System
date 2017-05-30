using System.Linq;
using System.Threading;
using EIS.Inventory.Core.Services;
using Microsoft.AspNet.SignalR;
using System;

namespace EIS.Inventory.Hubs
{
    public class SystemJobHub : Hub
    {
        private readonly ISystemJobService _service;

        public SystemJobHub(ISystemJobService service)
        {
            _service = service;
        }

        public void GetUnNotifiedSystemJobs()
        {
            var systemJobs = _service.GetUnNotifiedSystemJobs();
            if (!systemJobs.Any())
                return;

            // send the system jobs to all listeners
            Clients.All.sendCurrentJobs(systemJobs);
            
            // set the notification for jobs to TRUE
            _service.SetNotifiedSystemJobs(systemJobs.Select(x => x.Id).ToList());
        }

        public void GetInprogressSystemJobs()
        {
            var systemJobs = _service.GetInprogressSystemJobs();
            if (!systemJobs.Any())
                Clients.All.sendInprogressSystemJobs(null);

            // send the system jobs to all listeners
            Clients.All.sendInprogressSystemJobs(systemJobs.Select(x => new
            {
                JobType = x.JobType.ToString(),
                TotalValue = x.TotalNumOfItems,
                CurrentValue = x.CurrentNumOfItems,
                Percentage = Math.Round(((x.CurrentNumOfItems ?? 1.0) / (x.TotalNumOfItems ?? 1.0)) * 100.0),
                SubmittedBy = x.SubmittedBy,
            }));
        }

        public void GetServerDateTime()
        {
            Clients.All.sendServerDateTime(DateTime.Now);
        }
    }
}