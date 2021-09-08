# Unturned Images
A repository of images for assets in the game Unturned.

Images are available on the [images branch](https://github.com/SilKsPlugins/UnturnedImages/tree/images).

## Purpose

Unturned Images is a public, open-source repository of images of in-game assets to be used either by plugins or websites.

These are accessible via the [jsDeliver CDN](https://www.jsdelivr.com/?docs=gh). For an example, view the [Usage section](#Usage).

This project is public and free to anyone to be used by anyone and pull requests will be reviewed.

## Usage

To access images, it's recommended to use the [jsDeliver CDN](https://www.jsdelivr.com/?docs=gh). For item images, it's as simple as substituting the item ID in the following example.


When an image for the ID doesn't exist, the HTTP GET request will return a 404 error code.

Eaglefire image URL (item ID 4):
```
https://cdn.jsdelivr.net/gh/SilKsPlugins/UnturnedIcons@images/vanilla/items/4.png
```

## Plans

- [ ] Images of vehicles are planned, however a method of retrieving these images is being worked on.
- [ ] Modded assets are planned and will likely start with the most popular mods (i.e. Elver). After which pull requests to add images for other mods will be accepted.
