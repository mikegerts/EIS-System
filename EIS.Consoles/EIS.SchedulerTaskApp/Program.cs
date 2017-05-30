using System;
using System.Collections.Generic;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;
using EIS.SchedulerTaskApp.Repositories;
using EIS.Inventory.DAL.Database;
using EIS.Inventory.Shared.Helpers;
using EIS.SchedulerTaskApp.Helpers;
using EIS.SchedulerTaskApp.Models;

namespace EIS.SchedulerTaskApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            // configure the auto mappings
            AutoMapperConfig.CreateMappings();

            Console.Title = ConfigurationManager.AppSettings["AppName"].ToString();
            var taskManager = new TaskManager();
            var repository = new TaskRepository();

            var keepRunning = true;

            while (keepRunning)
            {
                Console.WriteLine("Reading scheduled tasks from database...");

                // retrieve the scheduled tasks that are enabled that are enabled
                var scheduledTasks = repository.GetCurrentScheduledTasks();
                foreach (var task in scheduledTasks)
                {
                    if (task.TaskType == CustomerScheduledTaskType.CUSTOMER_EXPORT_SKU)
                    {
                        if (!task.IsRunNow && !isTaskReadingTime(task))
                            continue;

                        if (isTaskAlreadyExecuted(task.LastExecutedOn))
                            continue;

                        // let's update first its last execution so it will not be executed again
                        taskManager.UpdateCustomerScheduledTaskExecution(task);

                        Task.Run(() =>
                        {
                            taskManager.ExecuteCustomerScheduledTask(task);
                        });
                    }
                    else
                    {
                        if (!task.IsRunNow && !isTaskReadingTime(task))
                            continue;

                        if (isTaskAlreadyExecuted(task.LastExecutedOn))
                            continue;

                        // let's update first its last execution so it will not be executed again
                        taskManager.UpdateScheduledTaskExecution(task);

                        Task.Run(() =>
                        {
                            taskManager.ExecuteScheduledTask(task);
                        });
                    }
                }

                // let's sleep the main thread for a while
                goToSleep();
            }

            Console.ReadKey();
        }

        private static void goToSleep()
        {
            var sleepTime = Convert.ToInt16(ConfigurationManager.AppSettings["SleepTime"]);
            try
            {
                Thread.Sleep(sleepTime * 1000);
            }
            catch { }
        }

        private static bool isTaskReadingTime(ScheduledTask task)
        {
            var today = DateTime.Now;
            var taskReadTime = task.StartDateTime.TimeOfDay;

            if (task.RecurrenceEnum == Recurrence.Hourly)
            {
                var timeDifference = DateTime.Now - task.LastExecutedOn.Value;
                return timeDifference.TotalHours >= task.OccurrAt;
            }
            else if (task.RecurrenceEnum == Recurrence.Daily)
            {
                return (hasDay(task.DaysList) && isTimeEqual(today.TimeOfDay, taskReadTime));
            }
            else if (task.RecurrenceEnum == Recurrence.Weekly)
            {
                // return false if the week number is not equal to the task lat execution
                if (!isWeekNumberEqual(today, task.LastExecutedOn.Value, task.OccurrAt))
                    return false;

                // otherwise, continue and check if the today is the running day
                return (hasDay(task.DaysList) && isTimeEqual(today.TimeOfDay, taskReadTime));
            }
            else if (task.RecurrenceEnum == Recurrence.Custom)
            {
                // return false if the day is not equal
                if (!isDayNumberEqual(today, task.LastExecutedOn.Value, task.OccurrAt))
                    return false;

                return isTimeEqual(today.TimeOfDay, taskReadTime);
            }
            else
            {
                throw new ArgumentException("Invalid recurrency value: " + task.Recurrence);
            }
        }

        private static bool isWeekNumberEqual(DateTime today, DateTime lastExecution, int weekNo)
        {
            var weeks = (today - lastExecution).TotalDays / 7;
            return weeks >= weekNo;
        }

        private static bool isDayNumberEqual(DateTime today, DateTime lastExecution, int dayNo)
        {
            var days = (today - lastExecution).TotalDays;
            return days >= dayNo;
        }

        private static bool hasDay(List<string> days)
        {
            var today = DateTime.Now.DayOfWeek.ToString();
            return days.Exists(day => day.Equals(today));
        }

        private static bool isTimeEqual(TimeSpan today, TimeSpan taskReadTime)
        {
            return (today.Hours == taskReadTime.Hours && today.Minutes == taskReadTime.Minutes);
        }

        private static bool isTaskAlreadyExecuted(DateTime? executionDate)
        {
            if (executionDate == null)
                return false;

            var today = DateTime.Now;
            return (today.Date == executionDate.Value.Date 
                && isTimeEqual(today.TimeOfDay, executionDate.Value.TimeOfDay));
        }
    }
}
