using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace UnturnedImages.Module.Workshop
{
    public static class WorkshopHelper
    {
        private const string WorkshopPathIndicator = "steamapps/workshop/content/304930/";

        private static readonly FieldInfo AssetOrigin = typeof(Asset).GetField("origin", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool IsWorkshop(Asset asset)
        {
            var origin = AssetOrigin.GetValue(asset);

            return origin is AssetOrigin assetOrigin && assetOrigin.workshopFileId != 0;
        }

        public static ulong GetWorkshopId(Asset asset)
        {
            var origin = AssetOrigin.GetValue(asset);

            return origin is AssetOrigin assetOrigin ? assetOrigin.workshopFileId : 0;
        }

        public static ulong GetWorkshopIdSafe(Asset asset)
        {
            return GetWorkshopId(asset);
        }

        public static ulong[] GetAllMods()
        {
            var mods = new List<ulong>();

            List<ItemAsset> itemAssets = new List<ItemAsset>();
            List<VehicleAsset> vehicleAssets = new List<VehicleAsset>();

            Assets.find(itemAssets);
            Assets.find(vehicleAssets);

            mods.AddRange(itemAssets.Select(GetWorkshopIdSafe).Distinct());
            mods.AddRange(vehicleAssets.Select(GetWorkshopIdSafe).Distinct());

            return mods.Distinct().ToArray();
        }
    }
}
