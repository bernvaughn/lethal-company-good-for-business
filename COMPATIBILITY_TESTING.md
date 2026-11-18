# Compatibility Testing Plan - GoodForBusiness Mod

## Overview
This document outlines the compatibility testing strategy for the GoodForBusiness mod with popular Lethal Company mods:
- **MoreCompany** - Increases player limit beyond 4
- **LateCompany** - Late join support + player limit increase
- **BrutalCompany** - New enemies, modified enemy spawning, difficulty changes
- **LethalLevelLoader** - Custom level/moon support
- Other top mods from Thunderstore

## Potential Compatibility Concerns

### 1. Enemy Registration & Spawning (BrutalCompany)
| Concern | Risk | Mitigation |
|---------|------|------------|
| Enemy type name collision | Low | Our enemy registered as "Slime" via LethalLib |
| PowerLevel conflicts | Low | Slime PowerLevel = 0 (passive), BrutalCompany adds aggressive enemies |
| Spawn weight interference | Low | Slime spawnWeight = 0 (handled by SlimeManager, not natural spawn) |
| Enemy AI modifications | Medium | SlimeAI inherits EnemyAI; BrutalCompany may patch EnemyAI methods |

**Test Cases:**
- [ ] BrutalCompany enemies spawn normally alongside slimes
- [ ] Slime hatching works on BrutalCompany moons
- [ ] No duplicate slime enemy types registered
- [ ] Slime AI behavior not affected by BrutalCompany AI patches

### 2. Player Count & Late Join (MoreCompany, LateCompany)
| Concern | Risk | Mitigation |
|---------|------|------------|
| `allPlayerScripts` array size | Low | Code iterates over all players dynamically |
| Late join slime sync | Medium | NetworkObject.Spawn() handles JIP; `RestoreSlimesFromSave` on level load |
| Config sync with more players | Low | CSync via LethalLib handles config sync |
| Feeding distance with more players | Low | Raycast-based feeding, distance check per player |

**Test Cases:**
- [ ] 6-8 players: eggs spawn, hatching works
- [ ] Late join: joining player sees existing slimes
- [ ] Late join: slime state (growth, position) correct
- [ ] Multiple players feeding same slime simultaneously
- [ ] Config changes sync to all 8 players

### 3. Custom Moons (LethalLevelLoader)
| Concern | Risk | Mitigation |
|---------|------|------------|
| Egg spawn on modded moons | Low | `EggRarityModded` config handles LethalLib `LevelTypes.Modded` |
| Slime navigation on custom NavMesh | Medium | SlimeAI uses NavMeshAgent; depends on level having valid NavMesh |
| Save/load on custom moons | Low | Save data is position-based, moon-agnostic |

**Test Cases:**
- [ ] Eggs spawn on LLL custom moons with configured rarity
- [ ] Slimes navigate correctly on custom level NavMesh
- [ ] Slimes persist across custom moon visits
- [ ] Daily abilities work on custom moons

### 4. General Mod Conflicts
| Mod Category | Potential Conflict | Test Focus |
|--------------|-------------------|------------|
| UI mods (LethalConfig, etc.) | Config menu integration | Config appears in mod menu, changes apply |
| QoL mods (SlotCount, etc.) | Inventory/ship storage patches | Egg hatching scans correct inventory slots |
| Enemy mods (Skinwalkers, etc.) | Enemy registration | No duplicate NetworkPrefab IDs |
| Terminal mods | Custom item scanning | Slime Egg, Slimed Beehive scan correctly |
| Ship mods | Ship storage hatching | `HatchEggsOnShip` works with modified storage |

## Networking Compatibility

### Netcode Patcher Version
- Our mod: Netcode 1.5.2, Unity 2022.3.9f1
- Must match host's patched assembly version
- **Action**: Verify all test clients use same netcode-patched DLL

### NetworkPrefab Registration
- Our mod registers 4 network prefabs via LethalLib `NetworkPrefabs.RegisterNetworkPrefab()`
- LethalLib handles prefab ID assignment
- **Risk**: ID conflicts if multiple mods register prefabs in different orders
- **Test**: Join game with multiple mods, verify no "NetworkPrefab not found" errors

### CSync Config Sync
- LethalLib provides CSync integration
- Config entries marked with `ConfigSync` attribute sync automatically
- **Test**: Host changes config, clients receive updated values

## Test Matrix

### Minimum Test Combinations
| Test ID | Mods | Players | Scenario |
|---------|------|---------|----------|
| C1 | Base game only | 1 | Baseline functionality |
| C2 | MoreCompany | 6 | High player count |
| C3 | LateCompany | 4 + JIP | Late join |
| C4 | BrutalCompany | 4 | Modified enemies |
| C5 | LethalLevelLoader | 4 | Custom moon |
| C6 | MoreCompany + BrutalCompany | 6 | Combo: players + enemies |
| C7 | LateCompany + LethalLevelLoader | 4 + JIP | Combo: JIP + custom moon |
| C8 | All four mods | 6 + JIP | Full combo |

### Per-Combination Test Checklist
For each test combination above:
- [ ] Mod loads without errors on host and all clients
- [ ] Config generates and syncs correctly
- [ ] Slime eggs spawn in dungeons on all vanilla + modded moons
- [ ] Eggs can be picked up, carried, stored on ship
- [ ] Quota reset: eggs hatch into correct slime variants
- [ ] Slimes follow players, bob, face players (configurable)
- [ ] Multiplayer: position/state synced across all clients
- [ ] Feeding scrap increases growth progress on all clients
- [ ] At 100% growth, slime grows visually and unlocks ability
- [ ] Equipment Slime: daily equipment crate spawn works
- [ ] Bee Lover Slime: daily beehive hunt/spawn works
- [ ] Save/load preserves slimes across sessions
- [ ] Late join: slimes visible with correct state
- [ ] No BepInEx errors/warnings related to GoodForBusiness
- [ ] No NetworkObject/NetworkPrefab errors
- [ ] No duplicate slimes/items spawned

## Known Compatibility Issues (To Verify)

### 1. BrutalCompany Enemy Spawning
BrutalCompany modifies `RoundManager.SpawnEnemies` and enemy selection logic.
- Our mod patches `RoundManager.SpawnScrapInLevel` (not enemies)
- Slime spawning is manual via `SlimeManager.RestoreSlimesFromSave`
- **Expected**: No conflict, but verify slimes don't count toward enemy limits

### 2. MoreCompany/LateCompany Player Array
MoreCompany modifies `StartOfRound.allPlayerScripts` capacity.
- Our code uses `foreach (var player in StartOfRound.Instance.allPlayerScripts)` - dynamic
- **Expected**: Works correctly, but verify with 6+ players

### 3. LethalLevelLoader Level Types
LLL adds new `LevelTypes` enum values or uses `LevelTypes.Modded`.
- Our config has `EggRarityModded` for `LevelTypes.Modded`
- **Expected**: Works if LLL registers moons as `Modded` type

### 4. Terminal/Scanner Mods
Mods like `ScannerTerminal` or `LethalTerminal` modify scan behavior.
- Our items registered via LethalLib with proper scan nodes
- **Expected**: Items appear in scanner/terminal normally

## Testing Procedure

### Pre-Test Setup
```bash
# 1. Build Release version with netcode-patch
dotnet build -c Release

# 2. Package with tcli
tcli build

# 3. Create r2modman profile for each test combination
# 4. Install all mods in the combination
# 5. Verify all mods load in BepInEx console
```

### During Test
Monitor BepInEx console for:
- `[GoodForBusiness]` log messages (spawning, hatching, feeding, abilities)
- `NetworkObject` warnings/errors
- `NetworkPrefab` warnings/errors
- `Harmony` patch conflicts
- `CSync` config sync messages

### Post-Test Verification
- [ ] Save file (`GoodForBusiness_slimes.json`) created and valid
- [ ] Save file loads correctly on next session
- [ ] No duplicate NetworkObjectId warnings in logs
- [ ] No "NetworkPrefab not found" warnings

## Troubleshooting Common Issues

| Issue | Likely Cause | Resolution |
|-------|--------------|------------|
| Slimes not visible to clients | NetworkPrefab registration order | Ensure LethalLib loads before our mod (BepInDependency) |
| Config not syncing | CSync version mismatch | Verify LethalLib version >= 0.15.0 |
| Eggs not spawning on LLL moons | LevelTypes.Modded not used | Check LLL moon registration, adjust EggRarityModded |
| Duplicate slimes on JIP | RestoreSlimesFromSave called twice | Verify RoundManagerLoadLevelPatch runs once |
| Bee Lover Slime pathfinding fails | Custom NavMesh issues | Teleport fallback should handle (enabled by default) |
| Equipment Crate not spawning | Item not registered | Verify LethalLib registration order |

## Acceptance Criteria

All test combinations (C1-C8) must pass:
- [ ] **C1**: Baseline - all features work in vanilla
- [ ] **C2**: MoreCompany - 6 players, no desyncs
- [ ] **C3**: LateCompany - JIP works, state correct
- [ ] **C4**: BrutalCompany - enemies + slimes coexist
- [ ] **C5**: LethalLevelLoader - custom moon egg spawn works
- [ ] **C6**: MoreCompany + BrutalCompany - both work together
- [ ] **C7**: LateCompany + LLL - JIP on custom moon works
- [ ] **C8**: All four - full compatibility

## Test Report Template

```
Test Session: [Date]
Tester: [Name]
Mod Version: [Version]
Game Version: [LC Version]
Test Combination: [C1-C8]

Results:
- Mod Load: PASS/FAIL
- Config Sync: PASS/FAIL
- Egg Spawn: PASS/FAIL (vanilla moons: Y/N, modded moons: Y/N)
- Egg Hatch: PASS/FAIL
- Slime Sync: PASS/FAIL
- Feeding/Growth: PASS/FAIL
- Equipment Slime Ability: PASS/FAIL
- Bee Lover Slime Ability: PASS/FAIL
- Save/Load: PASS/FAIL
- JIP: PASS/FAIL (if applicable)
- Errors: [List any GoodForBusiness-related errors]
- Desyncs: [List any observed desyncs]

Overall: PASS/FAIL
Notes:
```

## Future Compatibility Considerations

1. **LethalLib updates**: Pin LethalLib version in manifest; test on updates
2. **Netcode Patcher updates**: Re-patch on Unity/Netcode version changes
3. **BepInEx 6**: When released, update plugin base class if needed
4. **New popular mods**: Add to test matrix as they emerge
5. **Modded moon types**: If LLL adds new LevelTypes, add config entries

## Version Compatibility Matrix

| GoodForBusiness | BepInEx | LethalLib | Netcode | LC Version | Tested With |
|----------------|---------|-----------|---------|------------|-------------|
| 1.0.0 | 5.4.2100 | 0.15.0 | 1.5.2 | v50+ | MoreCompany, LateCompany, BrutalCompany, LLL |

*Update this matrix after each compatibility test session.*