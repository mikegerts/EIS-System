using System;
using System.Collections.Generic;
using System.Linq;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Shared.Helpers
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

        public static IEnumerable <KeyValuePair> GetEnumKeyValuePairList<T>()
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new KeyValuePair
                {
                    Id = Convert.ToInt32(e).ToString(),
                    Name = e.ToString()
                });
        }
    }
}
