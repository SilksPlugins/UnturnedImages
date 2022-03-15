using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;

namespace UnturnedImages.Module.Workshop
{
    public static class WorkshopHelper
    {
        private const string WorkshopPathIndicator = "steamapps/workshop/content/304930/";

        public static bool IsWorkshop(Asset asset)
        {
            return asset.assetOrigin == EAssetOrigin.WORKSHOP ||
                   asset.absoluteOriginFilePath.Contains(WorkshopPathIndicator);
        }

        public static uint GetWorkshopId(Asset asset)
        {
            var originFilePath = asset.absoluteOriginFilePath.Replace('\\', '/');

            var index = originFilePath.IndexOf(WorkshopPathIndicator, StringComparison.Ordinal);

            if (index < 0)
            {
                throw new Exception($"Workshop ID could not be found for asset {asset.id} ({asset.assetCategory}) ({asset.absoluteOriginFilePath})");
            }

            var cutStr = originFilePath.Substring(index + WorkshopPathIndicator.Length + 1);

            var workshopIdStr = new string(cutStr.TakeWhile(char.IsNumber).ToArray());

            if (!uint.TryParse(workshopIdStr, out var workshopId))
            {
                throw new Exception($"Workshop ID could not be parsed for asset {asset.id} ({asset.assetCategory}) ({asset.absoluteOriginFilePath})");
            }

            return workshopId;
        }

        public static uint GetWorkshopIdSafe(Asset asset)
        {
            return !IsWorkshop(asset) ? 0 : GetWorkshopId(asset);
        }

        public static uint[] GetAllMods()
        {
            var mods = new List<uint>();

            mods.AddRange(Assets.find(EAssetType.ITEM).Select(GetWorkshopIdSafe).Distinct());
            mods.AddRange(Assets.find(EAssetType.VEHICLE).Select(GetWorkshopIdSafe).Distinct());

            return mods.Distinct().ToArray();
        }
    }
}
