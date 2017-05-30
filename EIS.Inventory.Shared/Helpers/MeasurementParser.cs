using System;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Shared.Helpers
{
    public static class MeasurementParser
    {
        public static string ParseLengthUnit(string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return string.Empty;

            var loweredUnit = unit.ToLower();
            var eisUnit = unit;

            switch (loweredUnit)
            {
                case "inches":
                case "hundredths-inches":
                    eisUnit = "inches";
                    break;
                case "feet":
                case "hundredths-feet":
                    eisUnit = "feet";
                    break;
                case "millimeters":
                case "hundredths-millimeters":
                    eisUnit = "millimeters";
                    break;
                case "centimeters":
                case "hundredths-centimeters":
                    eisUnit = "centimeters";
                    break;
                case "decimeters":
                case "hundredths-decimeters":
                    eisUnit = "decimeters";
                    break;
                case "meters":
                case "hundredths-meters":
                    eisUnit = "meters";
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown length unit: \'{0}\'", unit));
            }

            return eisUnit;
        }

        public static decimal ParseMeasurementValue(Measurement mesurement)
        {
            if (mesurement.Value == 0)
                return 0;

            return mesurement.Unit.Contains("hundredths") ? (mesurement.Value / 100) : mesurement.Value;
        }

        public static string ParseMassUnit(string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return string.Empty;

            var loweredUnit = unit.ToLower();
            var eisUnit = unit;

            switch (loweredUnit)
            {
                case "pounds":
                case "hundredths-pounds":
                    eisUnit = "pounds";
                    break;
                case "ounces":
                case "hundredths-ounces":
                    eisUnit = "ounces";
                    break;
                case "grams":
                case "hundredths-grams":
                    eisUnit = "grams";
                    break;
                case "kilograms":
                case "hundredths-kilograms":
                    eisUnit = "kilograms";
                    break;
                case "milligrams":
                case "hundredths-milligrams":
                    eisUnit = "milligrams";
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown mass unit: \'{0}\'", unit));
            }

            return eisUnit;
        }
    }
}
