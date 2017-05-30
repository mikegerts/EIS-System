using System;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using EIS.Inventory.Shared.Models;
using EIS.SystemJobApp.Models;
using EIS.SystemJobApp.Repositories;

namespace EIS.SystemJobApp.Workers
{
    public abstract class JobWorker
    {
        protected readonly SystemJob _systemJob;
        protected readonly LoggerRepository _logger;
        protected readonly SystemJobRepository _jobRepository;
        protected readonly BackgroundWorker _bw;
        protected readonly string _resultFileDirecctory;
        protected bool _isWorkerExecuted = false;
        private bool _isDone = false;
        protected bool _hasError = false;

        public JobWorker(SystemJob job)
        {
            _systemJob = job;
            _logger = new LoggerRepository();
            _jobRepository = new SystemJobRepository();
            _resultFileDirecctory = ConfigurationManager.AppSettings["SystemJobsResultFileRoot"].ToString();

            // init the Background Worker
            _bw = new BackgroundWorker();
            _bw.DoWork += new DoWorkEventHandler(bw_DoWork);
            _bw.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);
            _bw.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            _bw.WorkerReportsProgress = true;
            _bw.WorkerSupportsCancellation = true;
        }

        protected abstract void bw_DoWork(object sender, DoWorkEventArgs e);

        protected abstract void DoPostWorkCompleted();

        public bool IsJobDone { get { return _isDone; } }

        public int JobId { get { return _systemJob.Id; } }

        public void StartJob()
        {
            _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Inprogress);
            _bw.RunWorkerAsync();
        }

        public void CancelJob()
        {
            if (_isDone) return;
                
            _bw.CancelAsync();
            _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Canceled);
        }
        
        protected void setTotalItemsProcessed(int totalItems)
        {
            _jobRepository.UpdateSystemJobTotalItemsProcessed(_systemJob.Id, totalItems);
        }

        protected virtual void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _jobRepository.UpdateSystemJobCurrentItemProcessed(_systemJob.Id, e.ProgressPercentage);
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (_hasError)
            {
                _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Failed);
                return;
            }

            _jobRepository.UpdateSystemJobStatus(_systemJob.Id, JobStatus.Completed);
            _isDone = true;

            // then do some clean ups
            deleteWorkerFilePassed();

            // do some task when the main job has been completed
            DoPostWorkCompleted();
        }

        private void deleteWorkerFilePassed()
        {
            // delete the file from the server drive
            try
            {
                if (!string.IsNullOrEmpty(_systemJob.Parameters))
                {
                    Console.WriteLine("Deleting file -> " + _systemJob.Parameters);
                    File.Delete(_systemJob.Parameters);
                    Console.WriteLine("Deleting file successfull!");
                }
            }
            catch
            {
                Console.WriteLine("Error in deleting file -> " + _systemJob.Parameters);
            }
        }
    }
}
