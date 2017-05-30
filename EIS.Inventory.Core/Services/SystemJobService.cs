using System;
using System.Collections.Generic;
using System.Linq;
using X.PagedList;
using AutoMapper;
using EIS.Inventory.Core.Helpers;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Models;
using EIS.Inventory.Shared.ViewModels;
using System.Configuration;

namespace EIS.Inventory.Core.Services
{
    public class SystemJobService : ISystemJobService
    {

        public IPagedList<SystemJobListDto> GetPagedSystemJobs(int page, int pageSize)
        {
            IPagedList<SystemJobListDto> results = null;
            using (var context = new EisInventoryContext())
            {
                results = context.systemjobs
                    .OrderByDescending(x => x.Id)
                    .ToPagedList(page, pageSize)
                    .ToMappedPagedList<systemjob, SystemJobListDto>();
            }
            return results;
        }

        public SystemJobDto GetSystemJob(int id)
        {
            var result = new SystemJobDto();
            using(var context = new EisInventoryContext())
            {
                var systemJob = context.systemjobs.FirstOrDefault(x => x.Id == id);
                result = Mapper.Map<SystemJobDto>(systemJob);
            }
            return result;
        }

        public List<SystemJobListDto> GetUnNotifiedSystemJobs()
        {
            List<SystemJobListDto> results;
            using (var context = new EisInventoryContext())
            {
                var systemJobs = context.systemjobs.Where(x => !x.IsNotified).ToList();
                results = Mapper.Map<List<SystemJobListDto>>(systemJobs);
            }
            return results;
        }

        public List<SystemJobListDto> GetInprogressSystemJobs()
        {
            List<SystemJobListDto> results;
            using (var context = new EisInventoryContext())
            {
                var systemJobs = context.systemjobs.Where(x => x.Status == JobStatus.Inprogress).ToList();
                results = Mapper.Map<List<SystemJobListDto>>(systemJobs);
            }

            return results;
        }

        public string GetSystemJobParameterOut(int jobId)
        {
            var result = string.Empty;
            using (var context = new EisInventoryContext())
            {
                var systemJob = context.systemjobs.FirstOrDefault(x => x.Id == jobId);
                result = systemJob == null ? null : systemJob.ParametersOut;
            }

            return result;
        }

        public int CreateSystemJob(SystemJobDto model)
        {
            // create system job object
            var systemJob = Mapper.Map<systemjob>(model);

            using (var context = new EisInventoryContext())
            {
                systemJob.Created = DateTime.UtcNow;

                context.systemjobs.Add(systemJob);
                context.SaveChanges();                
            }

            return systemJob.Id;
        }

        public void SetNotifiedSystemJobs(List<int> jobIds)
        {
            if (jobIds == null || !jobIds.Any())
                return;

            using (var context = new EisInventoryContext())
            {
                var systemJobs = context.systemjobs
               .Where(x => jobIds.Contains(x.Id))
               .ToList();

                // update to IsNotified to TRUE and save changes
                systemJobs.ForEach(job => job.IsNotified = true);
                context.SaveChanges();
            }
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose other managed resources.
            }
            //release unmanaged resources.
        }
        #endregion
    }
}
