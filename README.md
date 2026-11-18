# GoodForBusiness

A Lethal Company mod that adds slime eggs hidden in generated dungeons. These hatch into colorful slimes with unique abilities that players can feed scrap to grow and unlock powerful daily abilities.

## Installation

1. Install [BepInEx](https://valkyrie.atlassian.net/wiki/spaces/BEXA/pages/513314819/Installing+BepInEx+pack) for Lethal Company.
2. Install [LethalLib](https://thunderstore.io/c/lethal-company/p/Evaisa/LethalLib/).
3. Download the latest release from Thunderstore or GitHub.
4. Place the `GoodForBusiness.dll` in your `BepInEx/plugins/` folder.
5. Launch the game. A config file will be generated at `BepInEx/config/com.bernvaughn.GoodForBusiness.cfg`.

## Features

- **Slime Eggs**: Rare spawns in dungeons that hatch into slimes at quota reset.
- **Equipment Slime** (Blue): Produces 1 random equipment item every day.
- **Bee Lover Slime** (Yellow/Black): Hunts beehives and produces Slimed Beehives.
- **Configurable**: Spawn rates, growth costs, drop tables, and more via BepInEx config.
- **Multiplayer Compatible**: Synced via Unity Netcode.

## Development

### Build

```bash
dotnet build GoodForBusiness.csproj --configuration Release
```

### Project Structure

```
src/          - Mod source code
Tests/        - Unit tests
.github/      - CI/CD workflows

Thunderstore package files live at the repository root: `manifest.json`,
`icon.png` (256x256), `README.md` and `thunderstore.toml`.
```

## Compatibility

This mod has been tested and is compatible with the following popular mods:

| Mod | Status | Notes |
|-----|--------|-------|
| [MoreCompany](https://thunderstore.io/c/lethal-company/p/MoreCompany/) | ✅ Compatible | Supports 6+ players |
| [LateCompany](https://thunderstore.io/c/lethal-company/p/LateCompany/) | ✅ Compatible | Supports late join |
| [BrutalCompany](https://thunderstore.io/c/lethal-company/p/BrutalCompany/) | ✅ Compatible | Enemy changes don't affect slimes |
| [LethalLevelLoader](https://thunderstore.io/c/lethal-company/p/LethalLevelLoader/) | ✅ Compatible | Custom moons supported via `EggRarityModded` config |
| [LethalConfig](https://thunderstore.io/c/lethal-company/p/LethalConfig/) | ✅ Compatible | Config menu integration |
| [SlotCount](https://thunderstore.io/c/lethal-company/p/SlotCount/) | ✅ Compatible | Inventory expansion works with eggs |

**Note**: Compatibility is tested with the versions listed in the manifest. New versions of these mods may require re-testing.

## Configuration

All settings can be configured via the BepInEx config file at `BepInEx/config/com.bernvaughn.GoodForBusiness.cfg` or through the in-game LethalConfig menu (if installed).

Key settings:
- **Egg Spawning**: Per-moon rarity, global spawn chance multiplier
- **Equipment Slime**: Growth cost (default 300), equipment drop table (JSON)
- **Bee Lover Slime**: Growth cost (default 400), beehive value (default 175), detection range
- **Visuals**: Slime bobbing, face players toggle

## License

MIT License