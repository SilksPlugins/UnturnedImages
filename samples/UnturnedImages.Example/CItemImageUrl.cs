extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Drawing;
using UnturnedImages.API.Items;

namespace UnturnedImages.Example
{
    [UsedImplicitly]
    [Command("itemimageurl")]
    [CommandSyntax("<item ID>")]
    [CommandDescription("Gets the image URL for an item asset.")]
    public class CItemImageUrl : UnturnedCommand
    {
        private readonly IItemImageDirectoryAsync _itemImageDirectory;

        public CItemImageUrl(IItemImageDirectoryAsync itemImageDirectory,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _itemImageDirectory = itemImageDirectory;
        }


        protected override async UniTask OnExecuteAsync()
        {
            var itemId = await Context.Parameters.GetAsync<ushort>(0);

            var itemUrl = await _itemImageDirectory.GetItemImageUrlAsync(itemId);

            if (itemUrl == null)
            {
                await PrintAsync($"The image URL for item ID {itemId} could not be found.", Color.Red);

                return;
            }

            await PrintAsync($"The image URL for item ID {itemId} is {itemUrl}");

            if (Context.Actor is UnturnedUser user)
            {
                await UniTask.SwitchToMainThread();

                user.Player.Player.sendBrowserRequest($"The image for item ID {itemId}", itemUrl);
            }
        }
    }
}
