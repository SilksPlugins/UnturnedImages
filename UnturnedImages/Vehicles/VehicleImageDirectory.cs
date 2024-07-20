extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using JetBrainsAnnotations::JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenMod.API.Ioc;
using OpenMod.API.Prioritization;
using OpenMod.Core.Events;
using OpenMod.Core.Plugins.Events;
using SDG.Unturned;
using SilK.Unturned.Extras.Configuration;
using SilK.Unturned.Extras.Events;
using SmartFormat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnturnedImages.API.Vehicles;
using UnturnedImages.Configuration.Overrides;
using UnturnedImages.Ranges;
using UnturnedImages.Repositories;

namespace UnturnedImages.Vehicles
{
    /// <summary>
    /// Default implementation of <see cref="IVehicleImageDirectorySync"/>
    /// and <see cref="IVehicleImageDirectoryAsync"/> services.
    /// </summary>
    [UsedImplicitly]
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class VehicleImageDirectory : IVehicleImageDirectorySync, IVehicleImageDirectoryAsync,
        IInstanceEventListener<OpenModInitializedEvent>,
        IInstanceEventListener<PluginConfigurationChangedEvent>
    {
        private readonly IConfigurationAccessor<UnturnedImagesPlugin> _configuration;

        private string? _defaultVehicleRepository;
        private List<RepositoryOverride> _overrideRepositories;

        /// <summary>
        /// Constructs a vehicle image directory.
        /// </summary>
        /// <param name="configuration">Configuration accessor for the Unturned Images plugin.</param>
        public VehicleImageDirectory(IConfigurationAccessor<UnturnedImagesPlugin> configuration)
        {
            _configuration = configuration;

            _defaultVehicleRepository = null;
            _overrideRepositories = new List<RepositoryOverride>();

            ParseConfig();
        }

        private void ParseConfig()
        {
            var config = _configuration.GetNullableInstance();

            if (config == null)
            {
                return;
            }

            var overrides = new List<RepositoryOverride>();

            var vehicleOverridesConfig = config.Get<VehicleOverridesConfig>();

            foreach (var overrideConfig in vehicleOverridesConfig?.VehicleOverrides ?? Array.Empty<OverrideConfig>())
            {
                RepositoryOverride? @override = null;
                var repository = overrideConfig.Repository;

                if (!string.IsNullOrWhiteSpace(overrideConfig.Id))
                {
                    var range = RangeHelper.ParseMulti(overrideConfig.Id);
                    @override = new RepositoryOverride(range, repository);
                }
                else if(!string.IsNullOrWhiteSpace(overrideConfig.Guid) && Guid.TryParse(overrideConfig.Guid, out var guid))
                {
                    @override = new RepositoryOverride(guid, repository);
                }
                else if(!string.IsNullOrEmpty(overrideConfig.WorkshopId) && ulong.TryParse(overrideConfig.WorkshopId, out var workshopId))
                {
                    @override = new RepositoryOverride(workshopId.ToString(), repository);
                }

                if(@override != null)
                    overrides.Add(@override);
            }

            _defaultVehicleRepository = config.GetValue<string?>("DefaultRepositories:Vehicles", null);
            _overrideRepositories = overrides;
        }

        /// <inheritdoc />
        public UniTask HandleEventAsync(object? sender, OpenModInitializedEvent @event)
        {
            ParseConfig();

            return UniTask.CompletedTask;
        }

        /// <inheritdoc />
        public UniTask HandleEventAsync(object? sender, PluginConfigurationChangedEvent @event)
        {
            ParseConfig();

            return UniTask.CompletedTask;
        }

        /// <inheritdoc />
        public string? GetVehicleImageUrlSync(ushort id, bool includeWorkshop)
        {
            var asset = GetVehicleAsset(id, out var color);

            if (asset == null)
                return null;

            return GetVehicleImageUrlSync(asset, color, includeWorkshop);
        }


        /// <inheritdoc />
        public Task<string?> GetVehicleImageUrlAsync(ushort id, bool includeWorkshop)
        {
            return Task.FromResult(GetVehicleImageUrlSync(id, includeWorkshop));
        }

        /// <inheritdoc />
        public Task<string?> GetVehicleImageUrlAsync(Guid guid, bool includeWorkshop = true)
        {
            return Task.FromResult(GetVehicleImageUrlSync(guid, includeWorkshop));
        }

        /// <inheritdoc />
        public Task<string?> GetVehicleImageUrlAsync(Guid guid, Color32 paintColor, bool includeWorkshop = true)
        {
            return Task.FromResult(GetVehicleImageUrlSync(guid, paintColor, includeWorkshop));
        }

        /// <inheritdoc />
        public string? GetVehicleImageUrlSync(Guid guid, bool includeWorkshop = true)
        {
            var asset = GetVehicleAsset(guid, out var color);

            if (asset == null)
                return null;

            return GetVehicleImageUrlSync(asset, color, includeWorkshop);
        }

        /// <inheritdoc />
        public string? GetVehicleImageUrlSync(Guid guid, Color32 paintColor,bool includeWorkshop = true)
        {
            var asset = GetVehicleAsset(guid, out _);

            if (asset == null)
                return null;

            return GetVehicleImageUrlSync(asset, paintColor, includeWorkshop);
        }

        private static readonly FieldInfo AssetOrigin = typeof(Asset).GetField("origin", BindingFlags.Instance | BindingFlags.NonPublic);

        private string? GetVehicleImageUrlSync(VehicleAsset asset, Color32? paintColor, bool includeWorkshop = true)
        {
            if (AssetOrigin.GetValue(asset) is not AssetOrigin origin)
                return null;

            if (!includeWorkshop && origin.workshopFileId != 0)
            {
                return null;
            }

            var @override = _overrideRepositories.FirstOrDefault(x => x.Contains(asset.GUID, asset.id, origin.workshopFileId));

            var repository = @override?.Repository ?? _defaultVehicleRepository;

            string id = asset.GUID.ToString();

            // if paint color is null use 0 default paint color
            // if paint color is not null but not valid use 0 paint color
            if((paintColor == null && asset.SupportsPaintColor) ||
                (asset.SupportsPaintColor && paintColor != null && !asset.DefaultPaintColors.Any(x => x.r == paintColor.Value.r && x.g == paintColor.Value.g && x.b == paintColor.Value.b)))
            {
                paintColor = asset.DefaultPaintColors[0];
            }

            if(paintColor != null)
            {
                id += $"-{paintColor.Value.r}-{paintColor.Value.g}-{paintColor.Value.b}";
            }

            return repository == null ? null : Smart.Format(repository, new { VehicleId = id });
        }

        private VehicleAsset? GetVehicleAsset(ushort id, out Color32? color)
        {
            var asset = Assets.find(EAssetType.VEHICLE, id);

            if (asset == null)
            {
                color = null;
                return null;
            }

            return GetVehicleAsset(asset.GUID, out color);
        }

        private VehicleAsset? GetVehicleAsset(Guid guid, out Color32? color)
        {
            var asset = Assets.find(guid);
            color = null;

            if (asset is VehicleAsset vehicleAsset)
            {
                return vehicleAsset;
            }

            if (asset is RedirectorAsset redirectorAsset)
            {
                return Assets.find<VehicleAsset>(redirectorAsset.TargetGuid);
            }

            if(asset is VehicleRedirectorAsset vehicleRedirectorAsset)
            {
                color = vehicleRedirectorAsset.LoadPaintColor ?? vehicleRedirectorAsset.SpawnPaintColor;
                return vehicleRedirectorAsset.TargetVehicle.Find();
            }

            return null;
        }
    }
}
