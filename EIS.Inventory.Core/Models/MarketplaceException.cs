using System;
using System.Collections.Generic;

namespace EIS.Inventory.Core.Models
{
    public class MarketplaceException : Exception
    {
        /// <summary>
        /// The collection of results that errored out.
        /// </summary>
        public List<Result> ErrorResults { get; set; }

        public MarketplaceException(string message)
            : base(message)
        {
            ErrorResults = new List<Result>();
        }

        public class Result
        {
            public string Code { get; set; }
            public string Description { get; set; }
            public string SKU { get; set; }
        }
    }
}
