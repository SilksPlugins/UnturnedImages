extern alias JetBrainsAnnotations;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Triggers;
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
using System.Text;
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
            var asset = Assets.find(EAssetType.VEHICLE, id);

            if (asset == null)
                return null;

            Color32 paintColor = new Color(0,0,0,0);

            if(asset is VehicleRedirectorAsset redirectorAsset)
            {
                paintColor = redirectorAsset.SpawnPaintColor ?? redirectorAsset.LoadPaintColor ?? new Color32(0,0,0,0);
            }
            else if(asset is VehicleAsset vehicleAsset && vehicleAsset.SupportsPaintColor)
            {
                paintColor = vehicleAsset.DefaultPaintColors[0];
            }

            return GetVehicleImageUrlSync(asset.GUID, paintColor, includeWorkshop);
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
            return GetVehicleImageUrlSync(guid, new Color32(0, 0, 0, 0), includeWorkshop);
        }

        /// <inheritdoc />
        public string? GetVehicleImageUrlSync(Guid guid, Color32 paintColor,bool includeWorkshop = true)
        {
            var asset = Assets.find<VehicleAsset>(guid);

            if (asset == null)
                return null;

            return GetVehicleImageUrlSync(asset, paintColor, includeWorkshop);
        }

        private static readonly FieldInfo AssetOrigin = typeof(Asset).GetField("origin", BindingFlags.Instance | BindingFlags.NonPublic);

        private string? GetVehicleImageUrlSync(VehicleAsset asset, Color32 paintColor, bool includeWorkshop = true)
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

            if(paintColor.r != 0 || paintColor.g != 0 || paintColor.b != 0)
            {
                id += $"-{paintColor.r}-{paintColor.g}-{paintColor.b}";
            }

            return repository == null ? null : Smart.Format(repository, new { VehicleId = id });
        }
    }
}
