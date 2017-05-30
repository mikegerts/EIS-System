
namespace EIS.Inventory.Shared.Models
{
    public class MediaContent
    {
        /// <summary>
        /// Gets or sets the id of the media
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Gets or sets the owner's ID of this media
        /// </summary>
        public string ParentId { get; set; }

        /// <summary>
        /// Gets or sets the URL(FileName) of the media
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the caption
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// Gets or sets the type of media
        /// </summary>
        public string Type { get; set; }
    }
}
