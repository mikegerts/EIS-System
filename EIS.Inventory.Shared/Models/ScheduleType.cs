
namespace EIS.Inventory.Shared.Models
{
    public enum ScheduleType
    {
        /// <summary>
        /// Delete a previously created report request schedue
        /// </summary>
        _NEVER_,

        /// <summary>
        /// Every 15 minutes
        /// </summary>
        _15_MINUTES_,

        /// <summary>
        /// Every 30 minutes
        /// </summary>
        _30_MINUTES_,

        /// <summary>
        /// Every hour
        /// </summary>
        _1_HOUR_,

        /// <summary>
        /// Every 2 hours
        /// </summary>
        _2_HOURS_,

        /// <summary>
        /// Every 4 hours
        /// </summary>
        _4_HOURS_,

        /// <summary>
        /// Every 8 hours
        /// </summary>
        _8_HOURS_,

        /// <summary>
        /// Every 12 hours
        /// </summary>
        _12_HOURS_,

        /// <summary>
        /// Every day
        /// </summary>
        _1_DAY_,

        /// <summary>
        /// Every 2 days
        /// </summary>
        _2_DAYS_,

        /// <summary>
        /// Every 3 days
        /// </summary>
        _72_HOURS_,

        /// <summary>
        /// Every week
        /// </summary>
        _1_WEEK_,

        /// <summary>
        /// Every 14 days
        /// </summary>
        _14_DAYS_,

        /// <summary>
        /// Every 15 days
        /// </summary>
        _15_DAYS_,

        /// <summary>
        /// Every 30 days
        /// </summary>
        _30_DAYS_,
    }
}
