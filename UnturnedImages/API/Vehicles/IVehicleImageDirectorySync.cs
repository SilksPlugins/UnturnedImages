using OpenMod.API.Ioc;
using System;
using UnityEngine;

namespace UnturnedImages.API.Vehicles
{
    /// <summary>
    /// An asynchronous vehicle image directory.
    /// </summary>
    [Service]
    public interface IVehicleImageDirectorySync
    {
        /// <summary>
        /// Gets the URL for the specified vehicle's image.
        /// </summary>
        /// <param name="guid">The guid of the vehicle asset.</param>
        /// <param name="includeWorkshop">Whether or not workshop assets should be included.</param>
        /// <returns>
        /// The URL for the specified vehicle's image if one exists, <c>null</c> otherwise.
        /// Result may not be null even if image URL leads to 404 for performance reasons.
        /// </returns>
        string? GetVehicleImageUrlSync(Guid guid, bool includeWorkshop = true);

        /// <summary>
        /// Gets the URL for the specified vehicle's image.
        /// </summary>
        /// <param name="guid">The guid of the vehicle asset.</param>
        /// <param name="paintColor">The paint color image you want to get</param>
        /// <param name="includeWorkshop">Whether or not workshop assets should be included.</param>
        /// <returns>
        /// The URL for the specified vehicle's image if one exists, <c>null</c> otherwise.
        /// Result may not be null even if image URL leads to 404 for performance reasons.
        /// </returns>
        string? GetVehicleImageUrlSync(Guid guid, Color32 paintColor, bool includeWorkshop = true);

        /// <summary>
        /// Gets the URL for the specified vehicle's image.
        /// </summary>
        /// <param name="id">The ID of the vehicle asset.</param>
        /// <param name="includeWorkshop">Whether or not workshop assets should be included.</param>
        /// <returns>
        /// The URL for the specified vehicle's image if one exists, <c>null</c> otherwise.
        /// Result may not be null even if image URL leads to 404 for performance reasons.
        /// </returns>
        [Obsolete]
        string? GetVehicleImageUrlSync(ushort id, bool includeWorkshop = true);
    }
}
