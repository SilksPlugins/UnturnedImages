using OpenMod.API.Ioc;
using System.Threading.Tasks;

namespace UnturnedImages.API.Vehicles
{
    /// <summary>
    /// An asynchronous vehicle image directory.
    /// </summary>
    [Service]
    public interface IVehicleImageDirectoryAsync
    {
        /// <summary>
        /// Gets the URL for the specified vehicle's image.
        /// </summary>
        /// <param name="id">The ID of the vehicle asset.</param>
        /// <param name="includeWorkshop">Whether or not workshop assets should be included.</param>
        /// <returns>
        /// The URL for the specified vehicle's image if one exists, <c>null</c> otherwise.
        /// Result may not be null even if image URL leads to 404 for performance reasons.
        /// </returns>
        Task<string?> GetVehicleImageUrlAsync(ushort id, bool includeWorkshop = true);
    }
}
