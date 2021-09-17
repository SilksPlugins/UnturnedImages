using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnturnedImages.Module.Images
{
    public static class ImageUtils
    {
        public static void CaptureVehicleImages(string outputSubpath, IEnumerable<VehicleAsset> vehicleAssets,
            Vector3? vehicleAngles = null)
        {
            foreach (var vehicleAsset in vehicleAssets)
            {
                var outputPath =
                    $"{ReadWrite.PATH}/Extras/{outputSubpath}/{vehicleAsset.originMasterBundle?.assetBundleNameWithoutExtension ?? "unknown"}/{vehicleAsset.id}";

                CustomVehicleTool.QueueVehicleIcon(vehicleAsset, outputPath, 1024, 1024, vehicleAngles);
            }
        }

        public static void CaptureAllVehicleImages(Vector3? vehicleAngles = null)
        {
            var vehicleAssets = Assets.find(EAssetType.VEHICLE).OfType<VehicleAsset>();

            CaptureVehicleImages("Vehicles", vehicleAssets, vehicleAngles);
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
                    texture =>
                    {
                        extraItemIconInfo.onItemIconReady(texture);

                        UnturnedLog.info(extraItemIconInfo.extraPath);
                        UnturnedLog.info(itemAsset.originMasterBundle?.assetBundleName);
                    });

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
