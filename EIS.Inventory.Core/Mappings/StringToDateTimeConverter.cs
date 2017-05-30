using System;
using AutoMapper;

namespace EIS.Inventory.Core.Mappings
{
    public class StringToDateTimeConverter : ITypeConverter<string, DateTime>
    {
        public DateTime Convert(string source, DateTime destination, ResolutionContext context)
        {
            if (source == null)
                return default(DateTime);

            DateTime dateTime;
            if (DateTime.TryParse(source, out dateTime))
                return dateTime;

            return default(DateTime);
        }
    }
}
