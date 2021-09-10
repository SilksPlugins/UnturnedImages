using HarmonyLib;
using JetBrains.Annotations;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace UnturnedImages.Module.UI
{
    public class UIManager
    {
        private static readonly FieldInfo IconToolsContainerField =
            AccessTools.Field(typeof(MenuWorkshopUI), "iconToolsContainer");

        private bool _isUIAttached;

        private ISleekElement? _iconToolsContainer;

        private readonly List<ISleekElement> _loadedElements;

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
                void AddElement<TElement>(Func<TElement> constructor, Action<TElement> modifiers)
                    where TElement : ISleekElement
                {
                    var element = constructor();

                    element.sizeOffset_X = 200;
                    element.sizeOffset_Y = 25;

                    modifiers(element);

                    _loadedElements.Add(element);
                    _iconToolsContainer.AddChild(element);
                }

                // Label - UnturnedImages Controls

                AddElement(Glazier.Get().CreateLabel, unturnedImagesVehiclesLabel =>
                {
                    unturnedImagesVehiclesLabel.positionOffset_Y = 175;
                    unturnedImagesVehiclesLabel.text = "UnturnedImages Controls";
                    unturnedImagesVehiclesLabel.fontAlignment = TextAnchor.MiddleCenter;
                });

                // Button - Export All Vehicle Icons

                AddElement(Glazier.Get().CreateButton, captureAllVehicleIconsButton =>
                {
                    captureAllVehicleIconsButton.positionOffset_Y = 200;
                    captureAllVehicleIconsButton.text = "Export All Vehicle Icons";
                    captureAllVehicleIconsButton.onClickedButton += OnClickedCaptureAllVehicleIconsButton;
                });

                // Button - Reload Module

                AddElement(Glazier.Get().CreateButton, reloadModuleButton =>
                {
                    reloadModuleButton.positionOffset_Y = 225;
                    reloadModuleButton.text = "Reload Module";
                    reloadModuleButton.onClickedButton += OnClickedReloadModule;
                });

                // Make workshop tools visible by default
                _iconToolsContainer.isVisible = true;
            }
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
        }

        private bool IsUnturnedUILoaded()
        {
            return MenuUI.window != null;
        }

        private void OnClickedCaptureAllVehicleIconsButton(ISleekElement button)
        {
            IconUtils.CreateExtrasDirectory();
            VehicleIconUtils.CaptureAllVehicleIcons();
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
