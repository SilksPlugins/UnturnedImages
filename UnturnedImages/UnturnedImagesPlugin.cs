using JetBrains.Annotations;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

[assembly: PluginMetadata("UnturnedImages", DisplayName = "Unturned Images", Author = "SilK")]

namespace UnturnedImages
{
    /// <summary>
    /// Plugin for Unturned Images services.
    /// </summary>
    [UsedImplicitly]
    public class UnturnedImagesPlugin : OpenModUnturnedPlugin
    {
        /// <summary>
        /// Used by OpenMod.
        /// </summary>
        public UnturnedImagesPlugin(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
