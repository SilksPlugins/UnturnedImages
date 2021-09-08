using OpenMod.API.Ioc;
using System.Threading.Tasks;

namespace UnturnedImages.API.Items
{
    /// <summary>
    /// An asynchronous item image directory.
    /// </summary>
    [Service]
    public interface IItemImageDirectoryAsync
    {
        /// <summary>
        /// Gets the URL for the specified item's image.
        /// </summary>
        /// <param name="id">The ID of the item asset.</param>
        /// <param name="includeWorkshop">Whether or not workshop assets should be included.</param>
        /// <returns>
        /// The URL for the specified item's image if one exists, <c>null</c> otherwise.
        /// Result may not be null even if image URL leads to 404 for performance reasons.
        /// </returns>
        Task<string?> GetItemImageUrlAsync(ushort id, bool includeWorkshop = true);
    }
}
