using SDG.Unturned;
using System.Linq;

namespace UnturnedImages.Module.Images
{
    public static class ImageUtils
    {
        public static void CaptureAllVehicleImages()
        {
            var vehicleAssets = Assets.find(EAssetType.VEHICLE).OfType<VehicleAsset>().ToList();

            foreach (var vehicleAsset in vehicleAssets)
            {
                CustomVehicleTool.QueueVehicleIcon(vehicleAsset, 1024, 1024);
            }
        }

        public static void CaptureAllItemImages()
        {
            var itemAssets = Assets.find(EAssetType.ITEM).OfType<ItemAsset>().ToList();

            foreach (var itemAsset in itemAssets)
            {
                var extraItemIconInfo = new ExtraItemIconInfo
                {
                    extraPath = $"{ReadWrite.PATH}/Extras/Items/{itemAsset.id}"
                };

                ItemTool.getIcon(itemAsset.id, 0, 100, itemAsset.getState(), itemAsset, null, string.Empty,
                    string.Empty, itemAsset.size_x * 512, itemAsset.size_y * 512, false, true,
                    extraItemIconInfo.onItemIconReady);

                IconUtils.extraIcons.Add(extraItemIconInfo);
            }
        }
    }
}
