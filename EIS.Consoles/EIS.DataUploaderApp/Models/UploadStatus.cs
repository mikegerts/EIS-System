using System;
using EIS.DataUploaderApp.Helpers;

namespace EIS.DataUploaderApp.Models
{
    public class UploadStatus
    {
        public long Id { get; set; }

        public int VendorId { get; set; }

        public DateTime? StartUploadDate { get; set; }

        public DateTime? EndUploadDate { get; set; }

        public StatusType StatusType { get; set; }

        public int Attempt { get; set; }
    }
}
