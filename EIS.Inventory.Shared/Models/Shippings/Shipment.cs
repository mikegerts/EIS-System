using System;
using System.Collections.Generic;
using System.Linq;

namespace EIS.Inventory.Shared.Models
{
    public class Shipment
    {
        public Shipment() { }
        public Shipment(Address originAddress, Address destinationAddress, Package package)
        {
            OriginAddress = originAddress;
            DestinationAddress = destinationAddress;
            Packages = new List<Package> { package };
        }

        public Shipment(Address originAddress, Address destinationAddress, List<Package> packages)
        {
            OriginAddress = originAddress;
            DestinationAddress = destinationAddress;
            Packages = packages;
        }

        /// <summary>
        /// This might be the EIS Order ID
        /// </summary>
        public string TransactionId { get; set; }
        public DateTime ShipDate { get; set; }
        public MailClass MailClass { get; set; }
        public PackageType PackageType { get; set; }
        public ConfirmationType ConfirmationType { get; set; }
        public InsuranceType InsuranceType { get; set; }
        public Address OriginAddress { get; set; }
        public Address DestinationAddress { get; set; }
        public List<Package> Packages { get; set; }
        public int PackageCount
        {
            get { return Packages.Count; }
        }
        public decimal TotalPackageWeight
        {
            get { return Packages.Sum(x => x.Weight); }
        }
    }
}
