using SDG.Unturned;
using System.Linq;
using UnturnedImages.Module.Images;

namespace UnturnedImages.Module
{
    public static class VehicleIconUtils
    {
        public static void CaptureAllVehicleIcons()
        {
            var vehicleAssets = Assets.find(EAssetType.VEHICLE).OfType<VehicleAsset>().ToList();

            foreach (var vehicleAsset in vehicleAssets)
            {
                CustomVehicleTool.QueueVehicleIcon(vehicleAsset, 512, 512);
            }
        }
    }
}
