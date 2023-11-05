using HarmonyLib;
using JetBrains.Annotations;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnturnedImages.Module.Workshop;

namespace UnturnedImages.Module.Images
{
    public static class ImageUtils
    {
        internal static Vector3 ItemIconRotation { get; private set; }

        private static string GenerateIdRanges(List<ushort> ids)
        {
            ids.Sort();

            var rangesBuilder = new StringBuilder();

            bool startedRange = false;
            int? prevId = null;

            foreach (var id in ids)
            {
                if (!prevId.HasValue)
                {
                    // First number
                    rangesBuilder.Append(id);
                    prevId = id;

                    continue;
                }

                // General condition

                if (prevId.Value == id - 1)
                {
                    // Continue range of numbers

                    if (!startedRange)
                    {
                        rangesBuilder.Append('-');
                        startedRange = true;
                    }
                }
                else
                {
                    // Print last num, semi colon, and current num
                    rangesBuilder.Append(prevId.Value);
                    rangesBuilder.Append(';');
                    rangesBuilder.Append(id);

                    startedRange = false;
                }

                prevId = id;
            }

            if (startedRange && prevId.HasValue)
            {
                rangesBuilder.Append(prevId.Value);
            }

            return rangesBuilder.ToString();
        }

        private static void CaptureImages<TAsset>(
            IEnumerable<TAsset> assets, string outputCategory, Action<TAsset, string> exportAction) where TAsset : Asset
        {
            var basePath = Path.Combine(ReadWrite.PATH, "Extras", outputCategory);

            var modAssets = new Dictionary<uint, List<ushort>>();

            foreach (var asset in assets)
            {
                string modPathSection;

                if (WorkshopHelper.IsWorkshop(asset))
                {
                    var modId = WorkshopHelper.GetWorkshopId(asset);

                    modPathSection = Path.Combine("Workshop", modId.ToString());

                    if (modAssets.TryGetValue(modId, out var assetList))
                    {
                        assetList.Add(asset.id);
                    }
                    else
                    {
                        assetList = new List<ushort>()
                        {
                            asset.id
                        };

                        modAssets[modId] = assetList;
                    }
                }
                else
                {
                    modPathSection = "Official";
                }

                var fullPath = Path.Combine(basePath, modPathSection, asset.id.ToString());

                exportAction(asset, fullPath);
            }

            string assetCategory;

            if (typeof(TAsset) == typeof(ItemAsset))
            {
                assetCategory = "items";
            }
            else if (typeof(TAsset) == typeof(VehicleAsset))
            {
                assetCategory = "vehicles";
            }
            else
            {
                throw new ArgumentException($"Generic type {nameof(TAsset)} is not item or vehicle.");
            }

            foreach (var pair in modAssets)
            {
                var modId = pair.Key;
                var assetIds = pair.Value;

                var idRanges = GenerateIdRanges(assetIds);

                var directory = Path.Combine(basePath, "Workshop", modId.ToString());
                var fullPath = Path.Combine(directory, "config.yaml");

                UnturnedLog.info(fullPath);
                UnturnedLog.info("ID Ranges: " + idRanges);

                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using var streamWriter = new StreamWriter(fullPath, false);

                streamWriter.Write($@"
# Use this in your UnturnedImages/config.yaml file
- Id: ""{idRanges}"" # The ID of the override.
  Repository: ""https://cdn.jsdelivr.net/gh/SilKsPlugins/UnturnedIcons@images/modded/{modId}/{assetCategory}/{{ItemId}}.png"" # The repository of the override.
                ".Trim());
            }
        }

        public static void CaptureVehicleImages(IEnumerable<VehicleAsset> vehicleAssets,
            Vector3? vehicleAngles = null)
        {
            const string category = "Vehicles";

            CaptureImages(vehicleAssets, category, (asset, path) =>
            {
                CustomVehicleTool.QueueVehicleIcon(asset, path, 1024, 1024, vehicleAngles);
            });
        }

        public static void CaptureItemImages(IEnumerable<ItemAsset> itemAssets, Vector3? itemIconRotation = null)
        {
            const string category = "Items";
            
            // Update item icon rotation
            ItemIconRotation = itemIconRotation ?? Vector3.zero;

            CaptureImages(itemAssets, category, (asset, path) =>
            {
                var extraItemIconInfo = new ExtraItemIconInfo
                {
                    extraPath = path
                };

                ItemTool.getIcon(asset.id, 0, 100, asset.getState(), asset, null, string.Empty,
                    string.Empty, asset.size_x * 512, asset.size_y * 512, false, true,
                    texture =>
                    {
                        extraItemIconInfo.onItemIconReady(texture);
                    });

                IconUtils.extraIcons.Add(extraItemIconInfo);
            });
        }

        public static void CaptureAllVehicleImages(Vector3? vehicleAngles = null)
        {
            var vehicleAssets = Assets.find(EAssetType.VEHICLE).OfType<VehicleAsset>();

            CaptureVehicleImages(vehicleAssets, vehicleAngles);
        }

        public static void CaptureAllItemImages(Vector3? itemAngles = null)
        {
            var itemAssets = Assets.find(EAssetType.ITEM).OfType<ItemAsset>().ToList();

            CaptureItemImages(itemAssets, itemAngles);
        }

        public static void CaptureModItemImages(uint mod, Vector3? itemAngles = null)
        {
            var itemAssets = Assets.find(EAssetType.ITEM).OfType<ItemAsset>()
                .Where(x => WorkshopHelper.GetWorkshopIdSafe(x) == mod);

            CaptureItemImages(itemAssets, itemAngles);
        }

        public static void CaptureModVehicleImages(uint mod, Vector3? vehicleAngles = null)
        {
            var vehicleAssets = Assets.find(EAssetType.VEHICLE).OfType<VehicleAsset>()
                .Where(x => WorkshopHelper.GetWorkshopIdSafe(x) == mod);

            CaptureVehicleImages(vehicleAssets, vehicleAngles);
        }

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [HarmonyPatch]
        private static class UnturnedPatches
        {
            private static Vector3 _iconPosition;
            private static Quaternion _iconRotation;

            [HarmonyPatch(typeof(ItemTool), "captureIcon")]
            [HarmonyPrefix]
            public static void ItemToolCaptureIconPre(Transform model, Transform icon, ushort id, int width, int height, ref float orthoSize)
            {
                // Preserve position and rotation
                _iconPosition = icon.position;
                _iconRotation = icon.rotation;

                var up = icon.up;
                var forward = icon.forward;
                var right = icon.right;

                // Adjust item icon rotation
                icon.RotateAround(model.position, right, ItemIconRotation.x);
                icon.RotateAround(model.position, up, ItemIconRotation.y);
                icon.RotateAround(model.position, forward, ItemIconRotation.z);


                // Fix ortho size
                var itemAsset = Assets.find(EAssetType.ITEM, id);

                orthoSize = CustomImageTool.CalculateOrthographicSize(itemAsset, model.gameObject, icon, width, height, out var position);

                // Adjust item icon position
                icon.position = position;
            }

            [HarmonyPatch(typeof(ItemTool), "captureIcon")]
            [HarmonyPostfix]
            public static void ItemToolCaptureIconPost(Transform icon)
            {
                // Restore position and rotation
                icon.position = _iconPosition;
                icon.rotation = _iconRotation;
            }
        }
    }
}
