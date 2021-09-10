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
using UnturnedImages.API.Items;
using UnturnedImages.Configuration.Overrides;
using UnturnedImages.Ranges;

namespace UnturnedImages.Items
{
    /// <summary>
    /// Default implementation of <see cref="IItemImageDirectorySync"/>
    /// and <see cref="IItemImageDirectoryAsync"/> services.
    /// </summary>
    [UsedImplicitly]
    [ServiceImplementation(Lifetime = ServiceLifetime.Singleton, Priority = Priority.Lowest)]
    public class ItemImageDirectory : IItemImageDirectorySync, IItemImageDirectoryAsync,
        IInstanceEventListener<OpenModInitializedEvent>,
        IInstanceEventListener<PluginConfigurationChangedEvent>
    {
        internal class RepositoryOverride
        {
            public MultiRange Range { get; }

            public string Repository { get; }

            public RepositoryOverride(MultiRange range, string repository)
            {
                Range = range;
                Repository = repository;
            }
        }

        private readonly IConfigurationAccessor<UnturnedImagesPlugin> _configuration;

        private string? _defaultItemRepository;
        private List<RepositoryOverride> _overrideRepositories;

        /// <summary>
        /// Constructs an item image directory.
        /// </summary>
        /// <param name="configuration">Configuration accessor for the Unturned Images plugin.</param>
        public ItemImageDirectory(IConfigurationAccessor<UnturnedImagesPlugin> configuration)
        {
            _configuration = configuration;

            _defaultItemRepository = null;
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

            var itemOverridesConfig = config.Get<ItemOverridesConfig>();

            foreach (var overrideConfig in itemOverridesConfig.ItemOverrides)
            {
                var range = RangeHelper.ParseMulti(overrideConfig.Id);
                var repository = overrideConfig.Repository;

                var @override = new RepositoryOverride(range, repository);

                overrides.Add(@override);
            }

            _defaultItemRepository = config.GetValue<string?>("DefaultRepositories:Items", null);
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
        public string? GetItemImageUrlSync(ushort id, bool includeWorkshop)
        {
            if (!includeWorkshop)
            {
                // Verify item ID is not workshop

                if (Assets.find(EAssetType.ITEM, id) is not ItemAsset itemAsset ||
                    itemAsset.assetOrigin == EAssetOrigin.WORKSHOP)
                {
                    return null;
                }
            }

            var @override = _overrideRepositories.FirstOrDefault(x => x.Range.IsWithin(id));

            var repository = @override?.Repository ?? _defaultItemRepository;

            return repository == null ? null : Smart.Format(repository, new {ItemId = id});
        }


        /// <inheritdoc />
        public Task<string?> GetItemImageUrlAsync(ushort id, bool includeWorkshop)
        {
            return Task.FromResult(GetItemImageUrlSync(id, includeWorkshop));
        }
    }
}
