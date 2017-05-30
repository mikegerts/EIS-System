using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Shared.Helpers
{
    public static class EnumHelper
    {
        /// <summary>
        /// Parse the value into enum
        /// </summary>
        /// <typeparam name="T">The type of enum</typeparam>
        /// <param name="value">The string to convert to enum</param>
        /// <returns>Returns an Enum object</returns>
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, true);
        }

        /// <summary>
        /// Parse the string into enum
        /// </summary>
        /// <typeparam name="T">The type of enum</typeparam>
        /// <param name="value">The string value to convert to enum</param>
        /// <param name="defaultVal">The default for the enum if it fails to convert</param>
        /// <returns></returns>
        public static T ParseEnum<T>(string value, T defaultVal)
        {
            try { return (T)Enum.Parse(typeof(T), value, true); }
            catch { return defaultVal; }
        }

        /// <summary>
        /// Get the string value for the enum
        /// </summary>
        /// <param name="value">The enum value</param>
        /// <returns></returns>
        public static string GetStringValue(this Enum value)
        {
            // Get the type
            Type type = value.GetType();

            // Get fieldinfo for this type
            var fieldInfo = type.GetField(value.ToString());

            // Get the stringvalue attributes
            var attribs = fieldInfo.GetCustomAttributes(
                typeof(DisplayAttribute), false) as DisplayAttribute[];

            // Return the first if there was a match.
            return attribs.Length > 0 ? attribs[0].Name : value.ToString();
        }

        /// <summary>
        /// Convert the enum properties into list of string
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IEnumerable<T> GetList<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Get the desciption attribute assign to the enum
        /// </summary>
        /// <param name="value">The value of enum</param>
        /// <returns></returns>
        public static string GetDescription(this Enum value)
        {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if (name != null)
            {
                FieldInfo field = type.GetField(name);
                if (field != null)
                {
                    var attr =
                           Attribute.GetCustomAttribute(field,
                             typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (attr != null)
                    {
                        return attr.Description;
                    }
                }
            }

            // if we have no description attribute, just return the ToString of the enum
            return value.ToString();
        }

        public static IEnumerable<KeyValuePair> GetEnumKeyValuePairList<T>()
        {
            return Enum.GetValues(typeof(T))
                .Cast<T>()
                .Select(e => new KeyValuePair
                {
                    Id = Convert.ToInt32(e).ToString(),
                    Name =  e.ToString()
                });
        }
    }
}
