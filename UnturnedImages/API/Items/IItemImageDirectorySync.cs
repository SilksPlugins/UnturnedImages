using OpenMod.API.Ioc;
using System;

namespace UnturnedImages.API.Items
{
    /// <summary>
    /// A synchronous item image directory.
    /// </summary>
    [Service]
    public interface IItemImageDirectorySync
    {
        /// <summary>
        /// Gets the URL for the specified item's image.
        /// </summary>
        /// <param name="guid">The ID of the item asset.</param>
        /// <param name="includeWorkshop">Whether or not workshop assets should be included.</param>
        /// <returns>
        /// The URL for the specified item's image if one exists, <c>null</c> otherwise.
        /// Result may not be null even if image URL leads to 404 for performance reasons.
        /// </returns>
        string? GetItemImageUrlSync(Guid guid, bool includeWorkshop = true);


        /// <summary>
        /// Gets the URL for the specified item's image.
        /// </summary>
        /// <param name="id">The ID of the item asset.</param>
        /// <param name="includeWorkshop">Whether or not workshop assets should be included.</param>
        /// <returns>
        /// The URL for the specified item's image if one exists, <c>null</c> otherwise.
        /// Result may not be null even if image URL leads to 404 for performance reasons.
        /// </returns>
        [Obsolete]
        string? GetItemImageUrlSync(ushort id, bool includeWorkshop = true);
    }
}
