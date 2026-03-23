# RE9_ScopeResolutionFix

## Description
Scope resolution fix for RE9. Makes the scope render at full resolution and also improves performance when scoped compared to the base game (since the scope previously rendered at a higher FOV and zoomed/scaled that image instead of just zooming in with FOV).

## Dependencies
- REFrameworkNETPluginConfig https://github.com/TonWonton/REFrameworkNETPluginConfig

## Installation
### Lua
1. Install REFramework
  - NexusMods: https://www.nexusmods.com/residentevilrequiem/mods/13
  - GitHub: https://github.com/praydog/REFramework-nightly/releases
2. Download the lua script and extract to game folder
  - `RE9_ScopeResolutionFix.lua` should be in `\GAME_FOLDER\reframework\autorun\RE9_ScopeResolutionFix.lua`

### C#
1. Install prerequisites
  - REFramework + REFramework csharp-api (download and extract both `RE9.zip` AND `csharp-api.zip` to the game folder): https://github.com/praydog/REFramework-nightly/releases
    - Only extract `dinput8.dll` from the `RE9.zip`
  - .NET 10.0 Desktop Runtime x64: https://dotnet.microsoft.com/en-us/download/dotnet/10.0
2. Download the plugin and extract to game folder
  - `RE9_ScopeResolutionFix.dll` should be in `\GAME_FOLDER\reframework\plugins\managed\RE9_ScopeResolutionFix.dll`

- If the `csharp-api` is installed correctly a CMD window will pop up when launching the game
- The first startup after installing the `csharp-api` might take a while. Wait until it is complete. When the game isn't frozen anymore and it says "setting up script watcher" it is done
- The mod settings are under `REFramework.NET script generated UI` instead of the normal `Script generated UI`

## Features
- Scope is rendered at full quality/resolution
  - Improves performance compared to the base game when aiming with the scope
- Can change scope base FOV multiplier

## Changelog
### v1.0.0
- Initial release

### v1.0.1
- Add lua version
- Minor optimizations
