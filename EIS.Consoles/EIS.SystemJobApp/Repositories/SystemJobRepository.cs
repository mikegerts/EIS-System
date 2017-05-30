using System.Collections.Generic;
using System.Data;
using System.Linq;
using AutoMapper.QueryableExtensions;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.DAL.Database;
using EIS.SystemJobApp.Models;

namespace EIS.SystemJobApp.Repositories
{
    public class SystemJobRepository
    {
        public List<SystemJob> GetAllIncompleteJobs()
        {
            List<SystemJob> incompleteSystemJobs = null;
            using(var context = new EisInventoryContext())
            {
                incompleteSystemJobs = context.systemjobs
                    .Where(x => x.Status != JobStatus.Completed
                        && x.Status != JobStatus.Failed
                        && x.Status != JobStatus.Canceled)
                    .ProjectTo<SystemJob>()
                    .ToList();
            }

            return incompleteSystemJobs;
        }

        public void UpdateSystemJobStatus(int jobId, JobStatus status)
        {
            using (var context = new EisInventoryContext())
            {
                var systemJob = context.systemjobs.FirstOrDefault(x => x.Id == jobId);
                if (systemJob == null)
                    return;
                
                systemJob.IsNotified = false;
                systemJob.Status = status;
                context.SaveChanges();
            }
        }

        public void UpdateSystemJobCurrentItemProcessed(int jobId, int numOfItem)
        {
            using (var context = new EisInventoryContext())
            {
                var systemJob = context.systemjobs.FirstOrDefault(x => x.Id == jobId);
                if (systemJob == null)
                    return;

                systemJob.Status = JobStatus.Inprogress;
                systemJob.CurrentNumOfItems = numOfItem;
                context.SaveChanges();
            }
        }

        public void UpdateSystemJobTotalItemsProcessed(int jobId, int totalItems)
        {
            using (var context = new EisInventoryContext())
            {
                var systemJob = context.systemjobs.FirstOrDefault(x => x.Id == jobId);
                if (systemJob == null)
                    return;
                
                systemJob.TotalNumOfItems = totalItems;
                context.SaveChanges();
            }
        }

        public void UpdateSystemJobParametersOut(int jobId, string parameter)
        {
            using (var context = new EisInventoryContext())
            {
                var systemJob = context.systemjobs.FirstOrDefault(x => x.Id == jobId);
                if (systemJob == null)
                    return;
                
                systemJob.ParametersOut = parameter;
                context.SaveChanges();
            }
        }
    }
}
