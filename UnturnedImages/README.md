# Unturned Images Plugin
An OpenMod plugin for Unturned which adds a repository of images available to plugins for usage in UIs.

## Purpose

This plugin contains services which other plugins can use to get image URLs for in-game assets. For more information, see the [Usage section](#Usage).

These image URLs can be used in different applications, but mainly in-game UIs.

## Configuration

Default configuration (contains comment-based documentation):

```yaml
# Default repositories for different in-game asset categories.
# If no override specifies a different repository, it will attempt to use the default repository.
DefaultRepositories:
  # The default repository for item images.
  Items: "https://cdn.jsdelivr.net/gh/SilKsPlugins/UnturnedIcons@images/vanilla/items/{ItemId}.png"


# Repository overrides for item images. Simply specify an ID, or list of IDs and their repository.
# Remove the # before lines to uncomment them and have it work with the config.
# Priority is based on which line is first. For example, if all lines below were uncommented,
# the image URL for the item with an ID of 46001 would be https://cdn.jsdelivr.net/gh/SilKsPlugins/UnturnedIcons@images/modded/other2/items/{ItemId}.png
ItemOverrides:
#- Id: 46000 # The ID of the override.
#  Repository: "https://cdn.jsdelivr.net/gh/SilKsPlugins/UnturnedIcons@images/modded/other/items/{ItemId}.png" # The repository of the override.
#- Id: "46001-47000" # A range of IDs can be specified as well. You can specify multiple IDs using ranges. and multiple ranges by separating them with commas (',') or semi-colons (';').
#  Repository: "https://cdn.jsdelivr.net/gh/SilKsPlugins/UnturnedIcons@images/modded/other2/items/{ItemId}.png"
#- Id: "46001-47000;48000-49000;50023" # A range of IDs can be specified as well. You can specify multiple ranges/IDs by separating them with commas (',') or semi-colons (';').
#  Repository: "https://cdn.jsdelivr.net/gh/SilKsPlugins/UnturnedIcons@images/modded/other3/items/{ItemId}.png"
```

## Usage

Developers can include this plugin library in their plugins simply by adding the package to their project references and ensuring the plugin is installed on their server endpoints.

Install via Microsoft Visual Studio's Package Manager Console:
```
Install-Package SilK.UnturnedImages
```

Install via the .NET CLI:
```
dotnet add package SilK.UnturnedImages
```

After the plugin's library has been added to your project, you can retrieve one of the following services using dependency-injection:

- `IItemImageDirectoryAsync` - An asynchronous item image directory.
- `IItemImageDirectorySync` - A synchronous item image directory.