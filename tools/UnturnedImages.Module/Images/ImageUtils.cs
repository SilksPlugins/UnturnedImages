using SDG.Unturned;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnturnedImages.Module.Workshop;

namespace UnturnedImages.Module.Images
{
    public static class ImageUtils
    {
        public static void CaptureVehicleImages(IEnumerable<VehicleAsset> vehicleAssets,
            Vector3? vehicleAngles = null)
        {
            const string outputSubpath = "Vehicles";

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

            CaptureVehicleImages(vehicleAssets, vehicleAngles);
        }

        public static void CaptureItemImages(IEnumerable<ItemAsset> itemAssets)
        {
            const string outputSubpath = "Items";

            var basePath = $"{ReadWrite.PATH}/Extras/{outputSubpath}";

            foreach (var itemAsset in itemAssets)
            {
                var path = WorkshopHelper.IsWorkshop(itemAsset)
                    ? $"Workshop/{WorkshopHelper.GetWorkshopId(itemAsset)}"
                    : "Official";

                path = $"{basePath}/{path}/{itemAsset.id}";

                var extraItemIconInfo = new ExtraItemIconInfo
                {
                    extraPath = path
                };

                ItemTool.getIcon(itemAsset.id, 0, 100, itemAsset.getState(), itemAsset, null, string.Empty,
                    string.Empty, itemAsset.size_x * 512, itemAsset.size_y * 512, false, true,
                    texture =>
                    {
                        extraItemIconInfo.onItemIconReady(texture);

                        UnturnedLog.info(extraItemIconInfo.extraPath);
                    });

                IconUtils.extraIcons.Add(extraItemIconInfo);
            }
        }

        public static void CaptureAllItemImages()
        {
            var itemAssets = Assets.find(EAssetType.ITEM).OfType<ItemAsset>().ToList();

            CaptureItemImages(itemAssets);
        }

        public static void CaptureModItemImages(uint mod)
        {
            var itemAssets = Assets.find(EAssetType.ITEM).OfType<ItemAsset>()
                .Where(x => WorkshopHelper.GetWorkshopIdSafe(x) == mod);

            CaptureItemImages(itemAssets);
        }

        public static void CaptureModVehicleImages(uint mod, Vector3? vehicleAngles = null)
        {
            var vehicleAssets = Assets.find(EAssetType.VEHICLE).OfType<VehicleAsset>()
                .Where(x => WorkshopHelper.GetWorkshopIdSafe(x) == mod);

            CaptureVehicleImages(vehicleAssets, vehicleAngles);
        }
    }
}
