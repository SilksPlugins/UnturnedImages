using HarmonyLib;
using JetBrains.Annotations;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnturnedImages.Module.Images;
using UnturnedImages.Module.Workshop;

namespace UnturnedImages.Module.UI
{
    public class UIManager
    {
        private static readonly FieldInfo IconToolsContainerField =
            AccessTools.Field(typeof(MenuWorkshopUI), "iconToolsContainer");

        private bool _isUIAttached;

        private ISleekElement? _iconToolsContainer;

        private readonly List<ISleekElement> _loadedElements;

        private ISleekFloat32Field? _vehicleAnglesXInput;
        private ISleekFloat32Field? _vehicleAnglesYInput;
        private ISleekFloat32Field? _vehicleAnglesZInput;

        private ISleekFloat32Field? _itemAnglesXInput;
        private ISleekFloat32Field? _itemAnglesYInput;
        private ISleekFloat32Field? _itemAnglesZInput;

        public UIManager()
        {
            _loadedElements = new List<ISleekElement>();
        }

        public void Load()
        {
            UnturnedLog.info("UIManager loading");

            OnMenuUIStarted += AttachUI;

            if (IsUnturnedUILoaded())
            {
                AttachUI();
            }
        }

        public void Unload()
        {
            UnturnedLog.info("UIManager unloading");

            OnMenuUIStarted -= AttachUI;

            DetachUI();
        }

        private void AttachUI()
        {
            if (_isUIAttached)
            {
                return;
            }

            _isUIAttached = true;

            UnturnedLog.info("Attaching UI");

            _iconToolsContainer = (ISleekElement?)IconToolsContainerField.GetValue(null);

            if (_iconToolsContainer == null)
            {
                UnturnedLog.error("Could not find MenuWorkshopUI.iconToolsContainer");
            }
            else
            {
                var positionOffsetY = 300;

                void AddElement<TElement>(Func<TElement> constructor, Action<TElement> modifiers)
                    where TElement : ISleekElement
                {
                    var element = constructor();

                    element.sizeOffset_X = 200;
                    element.sizeOffset_Y = 25;

                    element.positionOffset_Y = positionOffsetY;
                    positionOffsetY += 25;

                    modifiers(element);

                    _loadedElements.Add(element);
                    _iconToolsContainer.AddChild(element);
                }

                // Label - UnturnedImages Controls

                AddElement(Glazier.Get().CreateLabel, unturnedImagesVehiclesLabel =>
                {
                    unturnedImagesVehiclesLabel.text = "UnturnedImages Controls";
                    unturnedImagesVehiclesLabel.fontAlignment = TextAnchor.MiddleCenter;
                });

                // Button - Export All Vehicle Images

                AddElement(Glazier.Get().CreateButton, captureAllVehicleIconsButton =>
                {
                    captureAllVehicleIconsButton.text = "Export All Vehicle Images";
                    captureAllVehicleIconsButton.onClickedButton += OnClickedCaptureAllVehicleImagesButton;
                });

                // Button - Export All Item Images

                AddElement(Glazier.Get().CreateButton, captureAllVehicleIconsButton =>
                {
                    captureAllVehicleIconsButton.text = "Export All Item Images";
                    captureAllVehicleIconsButton.onClickedButton += OnClickedCaptureAllItemImagesButton;
                });

                positionOffsetY += 25;

                // Button - Open Extras Folder

                AddElement(Glazier.Get().CreateButton, extrasFolderButton =>
                {
                    extrasFolderButton.text = "Open Extras Folder";
                    extrasFolderButton.onClickedButton += OnClickedOpenExtrasFolder;
                });

                // Button - Reload Module

                positionOffsetY += 25;

                AddElement(Glazier.Get().CreateButton, reloadModuleButton =>
                {
                    reloadModuleButton.text = "Reload Module";
                    reloadModuleButton.onClickedButton += OnClickedReloadModule;
                });

                // Label - Advanced Settings

                positionOffsetY += 25;

                AddElement(Glazier.Get().CreateLabel, advancedSettingsLabel =>
                {
                    advancedSettingsLabel.text = "Advanced Settings";
                });

                // Label - Item Icon Angles

                positionOffsetY += 25;

                AddElement(Glazier.Get().CreateLabel, vehicleIconAnglesLabel =>
                {
                    vehicleIconAnglesLabel.text = "Item Icon Angles";
                });

                // Item Icon Angles

                AddElement(Glazier.Get().CreateFloat32Field, itemAnglesYInput =>
                {
                    itemAnglesYInput.addLabel("X", ESleekSide.RIGHT);
                    itemAnglesYInput.state = 0;

                    _itemAnglesXInput = itemAnglesYInput;
                });

                AddElement(Glazier.Get().CreateFloat32Field, itemAnglesYInput =>
                {
                    itemAnglesYInput.addLabel("Y", ESleekSide.RIGHT);
                    itemAnglesYInput.state = 0;

                    _itemAnglesYInput = itemAnglesYInput;
                });

                AddElement(Glazier.Get().CreateFloat32Field, itemAnglesZInput =>
                {
                    itemAnglesZInput.addLabel("Z", ESleekSide.RIGHT);
                    itemAnglesZInput.state = -0;

                    _itemAnglesZInput = itemAnglesZInput;
                });

                // Label - Vehicle Icon Angles

                positionOffsetY += 25;

                AddElement(Glazier.Get().CreateLabel, vehicleIconAnglesLabel =>
                {
                    vehicleIconAnglesLabel.text = "Vehicle Icon Angles";
                });

                // Vehicle Icon Angles

                AddElement(Glazier.Get().CreateFloat32Field, vehicleAnglesXInput =>
                {
                    vehicleAnglesXInput.addLabel("X", ESleekSide.RIGHT);
                    vehicleAnglesXInput.state = 10;

                    _vehicleAnglesXInput = vehicleAnglesXInput;
                });

                AddElement(Glazier.Get().CreateFloat32Field, vehicleAnglesYInput =>
                {
                    vehicleAnglesYInput.addLabel("Y", ESleekSide.RIGHT);
                    vehicleAnglesYInput.state = 135;

                    _vehicleAnglesYInput = vehicleAnglesYInput;
                });

                AddElement(Glazier.Get().CreateFloat32Field, vehicleAnglesZInput =>
                {
                    vehicleAnglesZInput.addLabel("Z", ESleekSide.RIGHT);
                    vehicleAnglesZInput.state = -10;

                    _vehicleAnglesZInput = vehicleAnglesZInput;
                });

                // Label - Export Certain Mods

                AddElement(Glazier.Get().CreateLabel, exportCertainModsLabel =>
                {
                    exportCertainModsLabel.text = "Export Certain Mods";
                });

                // Buttons - Export Certain Mod

                foreach (var mod in WorkshopHelper.GetAllMods())
                {
                    AddElement(Glazier.Get().CreateButton, exportCertainModButton =>
                    {
                        exportCertainModButton.text = mod == 0 ? "Vanilla" : $"Mod {mod}";
                        exportCertainModButton.onClickedButton += x => OnExportModClicked(x, mod);
                    });
                }

                positionOffsetY += 25;

                // Make workshop tools visible by default
                _iconToolsContainer.isVisible = true;
            }
        }

        private void OnExportModClicked(ISleekElement button, uint modId)
        {
            var vehicleAngles = new Vector3(
                _vehicleAnglesXInput?.state ?? 0,
                _vehicleAnglesYInput?.state ?? 0,
                _vehicleAnglesZInput?.state ?? 0);

            var itemAngles = new Vector3(
                _itemAnglesXInput?.state ?? 0,
                _itemAnglesYInput?.state ?? 0,
                _itemAnglesZInput?.state ?? 0);

            IconUtils.CreateExtrasDirectory();
            ImageUtils.CaptureModItemImages(modId, itemAngles);
            ImageUtils.CaptureModVehicleImages(modId, vehicleAngles);
        }

        private void DetachUI()
        {
            if (!_isUIAttached)
            {
                return;
            }

            _isUIAttached = false;

            if (_iconToolsContainer != null)
            {
                foreach (var element in _loadedElements)
                {
                    _iconToolsContainer.RemoveChild(element);
                }
            }

            _vehicleAnglesXInput = null;
            _vehicleAnglesYInput = null;
            _vehicleAnglesZInput = null;

            _itemAnglesXInput = null;
            _itemAnglesYInput = null;
            _itemAnglesZInput = null;
        }

        private bool IsUnturnedUILoaded()
        {
            return MenuUI.window != null;
        }

        private void OnClickedCaptureAllVehicleImagesButton(ISleekElement button)
        {
            var vehicleAngles = new Vector3(
                _vehicleAnglesXInput?.state ?? 0,
                _vehicleAnglesYInput?.state ?? 0,
                _vehicleAnglesZInput?.state ?? 0);

            IconUtils.CreateExtrasDirectory();
            ImageUtils.CaptureAllVehicleImages(vehicleAngles);
        }

        private void OnClickedCaptureAllItemImagesButton(ISleekElement button)
        {
            var itemAngles = new Vector3(
                _itemAnglesXInput?.state ?? 0,
                _itemAnglesYInput?.state ?? 0,
                _itemAnglesZInput?.state ?? 0);

            IconUtils.CreateExtrasDirectory();
            ImageUtils.CaptureAllItemImages(itemAngles);
        }

        private void OnClickedOpenExtrasFolder(ISleekElement button)
        {
            var path = Path.Combine(ReadWrite.PATH, "Extras");

            Process.Start("explorer", path);
        }

        private void OnClickedReloadModule(ISleekElement button)
        {
            var bootstrapperAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(d => d.GetName().Name.Equals("UnturnedImages.Module.Bootstrapper"));

            var bootstrapperClass = bootstrapperAssembly!.GetType("UnturnedImages.Module.Bootstrapper.BootstrapperModule");
            var instanceProperty = bootstrapperClass.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
            var initializeMethod = bootstrapperClass.GetMethod("initialize", BindingFlags.Public | BindingFlags.Instance);
            var moduleInstance = instanceProperty!.GetValue(null);

            if (moduleInstance == null)
            {
                UnturnedLog.error("Could not find bootstrapper instance. Reload cancelled.");
                return;
            }

            UnturnedImagesModule.Instance!.shutdown();
            initializeMethod!.Invoke(moduleInstance, new object[0]);
        }

        public delegate void MenuUIStarted();

        public static event MenuUIStarted? OnMenuUIStarted;

        [UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
        [HarmonyPatch]
        private static class UnturnedPatches
        {
            [HarmonyPatch(typeof(MenuUI), "customStart")]
            [HarmonyPostfix]
            public static void MenuUICustomStart()
            {
                OnMenuUIStarted?.Invoke();
            }
        }
    }
}
