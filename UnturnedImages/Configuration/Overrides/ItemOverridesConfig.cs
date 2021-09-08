using System.Collections.Generic;

namespace UnturnedImages.Configuration.Overrides
{
    /// <summary>
    /// The config which specifies multiple override configs for item assets.
    /// </summary>
    public class ItemOverridesConfig
    {
        /// <summary>
        /// The multiple override configs for item assets.
        /// Priority is the lower the index, the more prioritized it is.
        /// </summary>
        public ICollection<OverrideConfig> Items { get; set; } = new List<OverrideConfig>();
    }
}
