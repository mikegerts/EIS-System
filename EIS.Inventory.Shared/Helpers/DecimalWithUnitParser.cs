using System;
using EIS.Inventory.Shared.Models;

namespace EIS.Inventory.Shared.Helpers
{
    public static class DecimalWithUnitParser
    {
        public static string ParseUnitValue(string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return string.Empty;

            var loweredUnit = unit.ToLower();
            var eisUnit = unit;

            switch (loweredUnit)
            {
                // for length
                case "inches":
                case "tenths-inches":
                case "hundredths-inches":
                    eisUnit = "inches";
                    break;
                case "feet":
                case "tenths-feet":
                case "hundredths-feet":
                    eisUnit = "feet";
                    break;
                case "millimeters":
                case "tenths-millimeters":
                case "hundredths-millimeters":
                    eisUnit = "millimeters";
                    break;
                case "centimeters":
                case "tenths-centimeters":
                case "hundredths-centimeters":
                    eisUnit = "centimeters";
                    break;
                case "decimeters":
                case "tenths-decimeters":
                case "hundredths-decimeters":
                    eisUnit = "decimeters";
                    break;
                case "meters":
                case "tenths-meters":
                case "hundredths-meters":
                    eisUnit = "meters";
                    break;

                    // for mass
                case "pounds":
                case "tenths-pounds":
                case "hundredths-pounds":
                    eisUnit = "pounds";
                    break;
                case "ounces":
                case "tenths-ounces":
                case "hundredths-ounces":
                    eisUnit = "ounces";
                    break;
                case "grams":
                case "tenths-grams":
                case "hundredths-grams":
                    eisUnit = "grams";
                    break;
                case "kilograms":
                case "tenths-kilograms":
                case "hundredths-kilograms":
                    eisUnit = "kilograms";
                    break;
                case "milligrams":
                case "tenths-milligrams":
                case "hundredths-milligrams":
                    eisUnit = "milligrams";
                    break;
                default:
                    throw new ArgumentException(string.Format("Unknown length unit: \'{0}\'", unit));
            }

            return eisUnit;
        }

        public static decimal ParseDecimalValue(decimal value, string unit)
        {
            if (string.IsNullOrEmpty(unit))
                return value;

            if (unit.Contains("tenths"))
                return value / 10.0m;
            else if (unit.Contains("hundredths"))
                return value / 100.0m;
            else
                return value;
        }
    }
}
