namespace EIS.Inventory.Shared.Models
{
    public class Address
    {
        public Address()
        {
            CountryCode = "US";
        }

        public Address(string city, string state, string postalCode, string countryCode) : this(null, null, city, state, postalCode, countryCode)
        {
        }

        public Address(string line1, string line2, string city, string state, string postalCode, string countryCode)
        {
            Line1 = line1;
            Line2 = line2;
            City = city;
            StateOrRegion = state;
            PostalCode = postalCode;
            CountryCode = countryCode;
            IsResidential = false;
        }

        public string Line1 { get; set; }
        public string Line2 { get; set; }
        public string CountryCode { get; set; }
        public string City { get; set; }
        public string StateOrRegion { get; set; }
        public string PostalCode { get; set; }
        public bool IsResidential { get; set; }
    }
}
