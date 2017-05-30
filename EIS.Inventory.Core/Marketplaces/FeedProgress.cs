
using System;
namespace StockManagement.Core.Marketplaces
{
    public class FeedProgress
    {
        /// <summary>
        /// The feed submission ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Gets or sets the feed processing status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the description
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the feed type
        /// </summary>
        public string FeedType { get; set; }

        /// <summary>
        /// Gets or sets the submitted date for the feed
        /// </summary>
        public string SubmittedDate { get; set; }

        /// <summary>
        /// Get or sets the processing started date
        /// </summary>
        public DateTime ProcessingStartDate { get; set; }

        /// <summary>
        /// Get or sets the processing completed date
        /// </summary>
        public DateTime ProcessingCompletedDate { get; set; }

        /// <summary>
        /// Gets or sets the progress status
        /// </summary>
        public bool IsDone { get; set; }
    }
}
