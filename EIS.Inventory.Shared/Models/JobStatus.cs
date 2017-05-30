using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EIS.Inventory.Shared.Models
{
    public enum JobStatus
    {
        Pending = 0,
        Inprogress = 1,
        Completed = 2,
        Canceled = 3,
        Failed = 4,
    }
}
