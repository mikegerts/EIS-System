using EIS.Inventory.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace EIS.SchedulerTaskApp.Models
{
    public class ScheduledTask
    {
        public int Id { get; set; }
        public string TaskType { get; set; }
        public string Name { get; set; }
        public DateTime StartDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public DateTime? LastExecutedOn { get; set; }
        public bool IsRunNow { get; set; }
        public string Recurrence { get; set; }
        public int OccurrAt { get; set; }
        public string Days { get; set; }

        public DateTime StartDateTime
        {
            get
            {
                var today = DateTime.Now;
                return new DateTime(today.Year, today.Month, today.Day, StartTime.Hours, StartTime.Minutes, StartTime.Seconds);
            }
        }

        public Recurrence RecurrenceEnum
        {
            get { return EnumHelper.ParseEnum<Recurrence>(Recurrence); }
        }

        public List<string> DaysList
        {
            get { return string.IsNullOrEmpty(Days) ? null : Days.Split(',').ToList(); }
        }

    }
}
