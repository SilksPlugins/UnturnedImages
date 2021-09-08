extern alias JetBrainsAnnotations;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.API.Plugins;
using OpenMod.Unturned.Plugins;
using System;

[assembly: PluginMetadata("UnturnedImages.Example", DisplayName = "Unturned Images Example")]

namespace UnturnedImages.Example
{
    [UsedImplicitly]
    public class UnturnedImagesExamplePlugin : OpenModUnturnedPlugin
    {
        public UnturnedImagesExamplePlugin(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }
    }
}
