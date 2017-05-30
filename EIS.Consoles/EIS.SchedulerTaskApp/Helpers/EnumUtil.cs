using System;

namespace EIS.SchedulerTaskApp.Helpers
{
    public static class EnumUtil
    {
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        public static T ParseEnum<T>(string value, T defaultVal)
        {
            try { return (T)Enum.Parse(typeof(T), value, true); }
            catch { return defaultVal; }
        }
    }
}
