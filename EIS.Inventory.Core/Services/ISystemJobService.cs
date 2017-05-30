using System;
using System.Collections.Generic;
using X.PagedList;
using EIS.Inventory.Shared.ViewModels;

namespace EIS.Inventory.Core.Services
{
    public interface ISystemJobService : IDisposable
    {
        /// <summary>
        /// Get all EIS system jobs
        /// </summary>
        /// <param name="page">The page number</param>
        /// <param name="pageSize">The page size</param>
        /// <returns></returns>
        IPagedList<SystemJobListDto> GetPagedSystemJobs(int page, int pageSize);

        /// <summary>
        /// Get the system jobs with the specified id
        /// </summary>
        /// <param name="id">The id of the system job</param>
        /// <returns></returns>
        SystemJobDto GetSystemJob(int id);

        /// <summary>
        /// Get all unnotified system jobs
        /// </summary>
        /// <returns></returns>
        List<SystemJobListDto> GetUnNotifiedSystemJobs();

        /// <summary>
        /// Get all in-progress system jobs
        /// </summary>
        /// <returns></returns>
        List<SystemJobListDto> GetInprogressSystemJobs();

        /// <summary>
        /// Get the sytem job's parameter out value by system job id
        /// </summary>
        /// <param name="jobId">The id of the system job</param>
        /// <returns></returns>
        string GetSystemJobParameterOut(int jobId);

        /// <summary>
        /// Create system job
        /// </summary>
        /// <param name="jobType">The type of system job</param>
        /// <param name="parameters">This is the parameters for the job execution</param>
        /// <param name="submittedBy">The invoker of the system job</param>
        int CreateSystemJob(SystemJobDto systemJob);

        /// <summary>
        /// Mark the system jobs notified
        /// </summary>
        /// <param name="jobIds">The list of IDs the system jobs</param>
        void SetNotifiedSystemJobs(List<int> jobIds);
    }
}
