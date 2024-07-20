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
using UnturnedImages.API.Items;
using UnturnedImages.Configuration.Overrides;
using UnturnedImages.Ranges;
using UnturnedImages.Repositories;

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

            foreach (var overrideConfig in itemOverridesConfig?.ItemOverrides ?? Array.Empty<OverrideConfig>())
            {
                RepositoryOverride? @override = null;
                var repository = overrideConfig.Repository;

                if (!string.IsNullOrWhiteSpace(overrideConfig.Id))
                {
                    var range = RangeHelper.ParseMulti(overrideConfig.Id);
                    @override = new RepositoryOverride(range, repository);
                }
                else if (!string.IsNullOrWhiteSpace(overrideConfig.Guid) && Guid.TryParse(overrideConfig.Guid, out var guid))
                {
                    @override = new RepositoryOverride(guid, repository);
                }
                else if (!string.IsNullOrEmpty(overrideConfig.WorkshopId) && ulong.TryParse(overrideConfig.WorkshopId, out var workshopId))
                {
                    @override = new RepositoryOverride(workshopId.ToString(), repository);
                }

                if (@override != null)
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
            var asset = GetItemAsset(id);

            if (asset == null)
                return null;

            return GetItemImageUrlSync(asset, includeWorkshop);
        }

        /// <inheritdoc />
        public string? GetItemImageUrlSync(Guid guid, bool includeWorkshop = true)
        {
            var asset = GetItemAsset(guid);

            if (asset == null) return null;

            return GetItemImageUrlSync(asset, includeWorkshop);
        }


        /// <inheritdoc />
        public Task<string?> GetItemImageUrlAsync(ushort id, bool includeWorkshop)
        {
            return Task.FromResult(GetItemImageUrlSync(id, includeWorkshop));
        }

        /// <inheritdoc />
        public Task<string?> GetItemImageUrlAsync(Guid guid, bool includeWorkshop = true)
        {
            return Task.FromResult(GetItemImageUrlSync(guid, includeWorkshop));
        }

        private static readonly FieldInfo AssetOrigin = typeof(Asset).GetField("origin", BindingFlags.Instance | BindingFlags.NonPublic);

        private string? GetItemImageUrlSync(Asset asset, bool includeWorkshop = true)
        {
            if (AssetOrigin.GetValue(asset) is not AssetOrigin origin)
                return null;

            if (!includeWorkshop && origin.workshopFileId != 0)
            {
                return null;
            }

            var @override = _overrideRepositories.FirstOrDefault(x => x.Contains(asset.GUID, asset.id, origin.workshopFileId));

            var repository = @override?.Repository ?? _defaultItemRepository;

            return repository == null ? null : Smart.Format(repository, new { ItemId = asset.GUID });
        }

        private ItemAsset? GetItemAsset(ushort id)
        {
            var asset = Assets.find(EAssetType.ITEM, id);

            if(asset == null) return null;

            return GetItemAsset(asset.GUID);
        }

        private ItemAsset? GetItemAsset(Guid guid)
        {
            var asset = Assets.find(guid);

            if(asset is ItemAsset itemAsset)
            {
                return itemAsset;
            }

            if(asset is RedirectorAsset redirectorAsset)
            {
                return Assets.find<ItemAsset>(redirectorAsset.TargetGuid);
            }

            return null;
        }
    }
}
