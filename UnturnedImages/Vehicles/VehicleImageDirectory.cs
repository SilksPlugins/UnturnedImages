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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            foreach (var overrideConfig in vehicleOverridesConfig.VehicleOverrides)
            {
                var range = RangeHelper.ParseMulti(overrideConfig.Id);
                var repository = overrideConfig.Repository;

                var @override = new RepositoryOverride(range, repository);

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
            if (!includeWorkshop)
            {
                // Verify vehicle ID is not workshop

                if (Assets.find(EAssetType.VEHICLE, id) is not VehicleAsset vehicleAsset ||
                    vehicleAsset.assetOrigin == EAssetOrigin.WORKSHOP)
                {
                    return null;
                }
            }

            var @override = _overrideRepositories.FirstOrDefault(x => x.Range.IsWithin(id));

            var repository = @override?.Repository ?? _defaultVehicleRepository;

            return repository == null ? null : Smart.Format(repository, new { VehicleId = id });
        }


        /// <inheritdoc />
        public Task<string?> GetVehicleImageUrlAsync(ushort id, bool includeWorkshop)
        {
            return Task.FromResult(GetVehicleImageUrlSync(id, includeWorkshop));
        }
    }
}
