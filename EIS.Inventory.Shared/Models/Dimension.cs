using EIS.Inventory.Shared.Helpers;

namespace EIS.Inventory.Shared.Models
{
    public class Dimension
    {
        public Dimension() { }

        public Dimension(decimal length, decimal width, decimal height, decimal weight)
        {
            var unit = "inches";
            Length = new DecimalWithUnit(length, unit);
            Width = new DecimalWithUnit(width, unit);
            Height = new DecimalWithUnit(height, unit);
            Weight = new DecimalWithUnit(weight, "ounces");
        }

        public Dimension(DecimalWithUnit length, DecimalWithUnit width, DecimalWithUnit height, DecimalWithUnit weight)
        {
            Length = length;
            Width = width;
            Height = height;
            Weight = weight;
        }
        public DecimalWithUnit Length { get; set; }
        public DecimalWithUnit Width { get; set; }
        public DecimalWithUnit Height { get; set; }
        public DecimalWithUnit Weight { get; set; }
    }

    public class DecimalWithUnit
    {
        private decimal _value;
        private string _unit;

        public DecimalWithUnit(decimal value, string unit)
        {
            _value = value;
            _unit = unit;
        }

        public decimal Value
        {
            get { return DecimalWithUnitParser.ParseDecimalValue(_value, _unit); }
        }

        public string Unit
        {
            get { return DecimalWithUnitParser.ParseUnitValue(_unit); }
        }
    }
}
