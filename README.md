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
Thunderstore/ - Thunderstore packaging config
.github/      - CI/CD workflows
```

## License

MIT License