# RE9_ScopeResolutionFix

## Description
Scope resolution fix for RE9. Makes the scope render at full resolution and also improves performance when scoped compared to the base game (since the scope previously rendered at a higher FOV and zoomed/scaled that image instead of just zooming in with FOV).

## Dependencies
- REFrameworkNETPluginConfig https://github.com/TonWonton/REFrameworkNETPluginConfig

## Prerequisites
- REFramework and the REFramework C# API (csharp-api) https://github.com/praydog/REFramework-nightly/releases
- .NET 10.0 Desktop Runtime https://dotnet.microsoft.com/en-us/download/dotnet/10.0

## Installation
1. Install prerequisites (download BOTH the `RE9.zip` AND `csharp-api` and extract to game folder) and install the .NET 10.0 Desktop Runtime if you don't have it installed
2. Download the plugin and extract to game folder
3. The first startup after installing the `csharp-api` might take a while. Wait until it is complete. When the game isn't frozen anymore and it says "setting up script watcher" it is done
4. Open the REFramework UI -> `REFramework.NET script generated UI` -> RE9_ScopeResolutionFix -> change FOV and settings

## Features
- Scope is rendered at full quality/resolution
  - Improves performance compared to the base game when aiming with the scope
- Can change scope base FOV multiplier

## Changelog
### v1.0.0
- Initial release