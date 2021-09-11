using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;

namespace UnturnedImages.Module.Images
{
    public static class ImageUtils
    {
        public static void CaptureVehicleImages(string outputSubpath, IEnumerable<VehicleAsset> vehicleAssets)
        {
            foreach (var vehicleAsset in vehicleAssets)
            {
                var outputPath =
                    $"{ReadWrite.PATH}/Extras/{outputSubpath}/{vehicleAsset.originMasterBundle?.assetBundleNameWithoutExtension ?? "unknown"}/{vehicleAsset.id}";

                CustomVehicleTool.QueueVehicleIcon(vehicleAsset, outputPath, 1024, 1024);
            }
        }

        public static void CaptureAllVehicleImages()
        {
            var vehicleAssets = Assets.find(EAssetType.VEHICLE).OfType<VehicleAsset>();

            CaptureVehicleImages("Vehicles", vehicleAssets);
        }

        public static void CaptureItemImages(string outputSubpath, IEnumerable<ItemAsset> itemAssets)
        {
            foreach (var itemAsset in itemAssets)
            {
                var extraItemIconInfo = new ExtraItemIconInfo
                {
                    extraPath = $"{ReadWrite.PATH}/Extras/{outputSubpath}/{itemAsset.originMasterBundle?.assetBundleNameWithoutExtension ?? "unknown"}/{itemAsset.id}"
                };

                ItemTool.getIcon(itemAsset.id, 0, 100, itemAsset.getState(), itemAsset, null, string.Empty,
                    string.Empty, itemAsset.size_x * 512, itemAsset.size_y * 512, false, true,
                    extraItemIconInfo.onItemIconReady);

                IconUtils.extraIcons.Add(extraItemIconInfo);
            }
        }

        public static void CaptureAllItemImages()
        {
            var itemAssets = Assets.find(EAssetType.ITEM).OfType<ItemAsset>().ToList();

            CaptureItemImages("Items", itemAssets);
        }
    }
}
