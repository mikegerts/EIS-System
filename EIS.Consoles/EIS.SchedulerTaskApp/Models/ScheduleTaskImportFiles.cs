using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.SchedulerTaskApp.Models
{
    public class ScheduleTaskImportFiles
    {
        public Int64 Id { get; set; }
        public int ScheduledTaskId { get; set; }
        public string FileName { get; set; }
        public DateTime Created { get; set; }
    }
}
