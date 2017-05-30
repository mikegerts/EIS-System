﻿using System;

namespace EIS.Inventory.Shared.Models
{
    /// <summary>
    /// The class representation for item or package for shipment
    /// </summary>
    public class Package
    {
        public Package() { }

        /// <summary>
        ///     Creates a new package object.
        /// </summary>
        /// <param name="length">The length of the package, in inches.</param>
        /// <param name="width">The width of the package, in inches.</param>
        /// <param name="height">The height of the package, in inches.</param>
        /// <param name="weight">The weight of the package, in pounds.</param>
        /// <param name="insuredValue">The insured-value of the package, in dollars.</param>
        /// <param name="container">A specific packaging from a shipping provider. E.g. "LG FLAT RATE BOX" for USPS</param>
        /// <param name="signatureRequiredOnDelivery">If true, will attempt to send this to the appropriate rate provider.</param>
        public Package(int length, int width, int height, int weight, decimal insuredValue, string container = null, bool signatureRequiredOnDelivery = false)
            : this(length, width, height, (decimal)weight, insuredValue, container, signatureRequiredOnDelivery)
        {
        }

        /// <summary>
        ///     Creates a new package object.
        /// </summary>
        /// <param name="length">The length of the package, in inches.</param>
        /// <param name="width">The width of the package, in inches.</param>
        /// <param name="height">The height of the package, in inches.</param>
        /// <param name="weight">The weight of the package, in pounds.</param>
        /// <param name="insuredValue">The insured-value of the package, in dollars.</param>
        /// <param name="container">A specific packaging from a shipping provider. E.g. "LG FLAT RATE BOX" for USPS</param>
        /// <param name="signatureRequiredOnDelivery">If true, will attempt to send this to the appropriate rate provider.</param>
        public Package(decimal length, decimal width, decimal height, decimal weight, decimal insuredValue, string container = null, bool signatureRequiredOnDelivery = false)
        {
            Length = length;
            Width = width;
            Height = height;
            Weight = weight;
            InsuredValue = insuredValue;
            Container = container;
            SignatureRequiredOnDelivery = signatureRequiredOnDelivery;
        }

        public decimal CalculatedGirth
        {
            get
            {
                var result = (Width * 2) + (Height * 2);
                return Math.Ceiling(result);
            }
        }
        public decimal Height { get; set; }
        public decimal InsuredValue { get; set; }
        public bool IsOversize { get; set; }
        public decimal Length { get; set; }
        public decimal RoundedHeight
        {
            get { return Math.Ceiling(Height); }
        }
        public decimal RoundedLength
        {
            get { return Math.Ceiling(Length); }
        }
        public decimal RoundedWeight
        {
            get { return Math.Ceiling(Weight); }
        }
        public decimal RoundedWidth
        {
            get { return Math.Ceiling(Width); }
        }
        public decimal Weight { get; set; }
        public decimal Width { get; set; }
        public string Container { get; set; }
        public bool SignatureRequiredOnDelivery { get; set; }        
    }
}
