using System.Collections.Generic;

namespace UnturnedImages.Configuration.Overrides
{
    /// <summary>
    /// The config which specifies multiple override configs for vehicle assets.
    /// </summary>
    public class VehicleOverridesConfig
    {
        /// <summary>
        /// The multiple override configs for vehicle assets.
        /// Priority is the lower the index, the more prioritized it is.
        /// </summary>
        public ICollection<OverrideConfig> VehicleOverrides { get; set; } = new List<OverrideConfig>();
    }
}
