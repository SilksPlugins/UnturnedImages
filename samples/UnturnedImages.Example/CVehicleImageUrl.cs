extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using JetBrainsAnnotations::JetBrains.Annotations;
using OpenMod.Core.Commands;
using OpenMod.Unturned.Commands;
using OpenMod.Unturned.Users;
using System;
using System.Drawing;
using UnturnedImages.API.Vehicles;

namespace UnturnedImages.Example
{
    [UsedImplicitly]
    [Command("vehicleimageurl")]
    [CommandSyntax("<vehicle ID>")]
    [CommandDescription("Gets the image URL for an vehicle asset.")]
    public class CVehicleImageUrl : UnturnedCommand
    {
        private readonly IVehicleImageDirectoryAsync _vehicleImageDirectory;

        public CVehicleImageUrl(IVehicleImageDirectoryAsync vehicleImageDirectory,
            IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _vehicleImageDirectory = vehicleImageDirectory;
        }


        protected override async UniTask OnExecuteAsync()
        {
            var vehicleId = await Context.Parameters.GetAsync<string>(0);
            if (!Guid.TryParse(vehicleId, out var guid))
                throw new CommandWrongUsageException(Context);

            var vehicleUrl = await _vehicleImageDirectory.GetVehicleImageUrlAsync(guid, true);

            if (vehicleUrl == null)
            {
                await PrintAsync($"The image URL for vehicle ID {vehicleId} could not be found.", Color.Red);

                return;
            }

            await PrintAsync($"The image URL for vehicle ID {vehicleId} is {vehicleUrl}");

            if (Context.Actor is UnturnedUser user)
            {
                await UniTask.SwitchToMainThread();

                user.Player.Player.sendBrowserRequest($"The image for vehicle ID {vehicleId}", vehicleUrl);
            }
        }
    }
}
