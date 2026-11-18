# Multiplayer Testing Plan - GoodForBusiness Mod

## Overview
This document outlines the multiplayer testing strategy for the GoodForBusiness mod, covering host/client parity, join-in-progress, and desync checks for 2-4 players.

## Networking Architecture Changes

### Host-Authoritative Systems
All game logic runs on the host only:
- **Egg Spawning**: `SlimeEggSpawner.SpawnEggsInLevel()` - Host only
- **Egg Hatching**: `StartOfRoundPatch.OnNewDayPatch()` - Host only
- **Daily Abilities**: `SlimeManager.OnNewDay()` - Host only
- **Save/Load**: `SlimeManager.SaveToDisk()` - Host only

### Network Synchronization
| Component | Sync Method |
|-----------|-------------|
| Slime position/rotation | `NetworkTransform` (interpolated) |
| Slime state (Idle/Following/Eating/Growing/Ability) | `NetworkVariable<int>` + `ClientRpc` |
| Growth progress (0.0-1.0) | `NetworkVariable<float>` + `ClientRpc` |
| IsGrown flag | `NetworkVariable<bool>` + `ClientRpc` |
| OwnerPlayerId | `NetworkVariable<int>` |
| Feeding action | `ServerRpc` (client→host) + `ClientRpc` (host→all) |
| Egg hatching | `ServerRpc` (client→host) + `ClientRpc` (effects) |
| Daily ability trigger | `ServerRpc` (host) + `ClientRpc` (effects) |
| Item spawning (crate/beehive) | `ServerRpc` (host) + `NetworkObject.Spawn()` |

### Network Prefabs Registered
- `BP_SlimeEgg.prefab` - Slime Egg scrap
- `BP_SlimedBeehive.prefab` - Slimed Beehive scrap
- `BP_EquipmentCrate.prefab` - Equipment Crate
- `BP_SlimeBase.prefab` - Base Slime enemy

## Test Scenarios

### 1. Basic Multiplayer (2 Players)
**Setup**: Host + 1 Client
**Tests**:
- [ ] Mod loads on both host and client without errors
- [ ] Config sync works (CSync via LethalLib)
- [ ] Eggs spawn in dungeon for host
- [ ] Client sees eggs at same positions
- [ ] Host picks up egg, client sees it in host's inventory
- [ ] Client picks up egg, host sees it in client's inventory
- [ ] Egg hatches at quota reset on both screens
- [ ] Slime follows nearest player on both screens
- [ ] Slime position/rotation synced smoothly
- [ ] Feeding scrap to slime works from both host and client
- [ ] Growth progress syncs to all clients
- [ ] Slime grows visually on all clients
- [ ] Daily ability triggers on host, effects play on all clients

### 2. 3-4 Player Testing
**Setup**: Host + 2-3 Clients
**Tests**:
- [ ] All above tests pass with more players
- [ ] Multiple slimes can exist simultaneously
- [ ] Each slime tracks its own growth/owner
- [ ] No performance issues with 4 players + multiple slimes
- [ ] Feeding from different players correctly attributes ownership

### 3. Join-In-Progress (JIP)
**Setup**: Start game with host, have clients join after:
- [ ] Level already loaded
- [ ] Slimes already hatched and grown
- [ ] Eggs already in player inventories/ship

**Tests**:
- [ ] Joining client receives all existing slimes via `NetworkObject.Spawn()`
- [ ] Slime state (position, growth, grown status) correct on join
- [ ] Eggs in inventories/ship visible to joining client
- [ ] Daily abilities that already triggered don't re-trigger for joining client
- [ ] Save data loads correctly for joining client

### 4. Host Migration / Reconnection
**Tests**:
- [ ] Client disconnects and reconnects - slime state preserved
- [ ] Host disconnects - game ends (expected for Lethal Company)
- [ ] Client reconnects after quota reset - daily abilities not duplicated

### 5. Desync Checks
**Tests**:
- [ ] Slime position never diverges > 0.5m between host/client
- [ ] Growth progress always identical across all clients
- [ ] IsGrown flag always identical
- [ ] Feeding from client correctly updates host then syncs back
- [ ] No duplicate slimes spawned
- [ ] No duplicate items spawned (equipment crates, beehives)
- [ ] Egg hatching only happens once per egg
- [ ] Daily abilities only trigger once per day per slime

### 6. Edge Cases
**Tests**:
- [ ] Client feeds slime while host is far away (distance validation)
- [ ] Multiple clients try to feed same slime simultaneously
- [ ] Slime grows exactly at growth threshold from multiple feedings
- [ ] Bee Lover Slime pathfinding with multiple players in different locations
- [ ] Equipment Slime drop table weights respected
- [ ] Config changes (disable slime type) - existing slimes persist but no new ones spawn
- [ ] Modded moons - egg spawn rarity works

## Test Commands / Checklist

### Pre-Test Setup
```bash
# 1. Build Release version with netcode-patch
dotnet build -c Release

# 2. Package with tcli
tcli build

# 3. Install via r2modman on all test machines
# 4. Ensure all players have same mod version and config
```

### During Test
```bash
# Host console commands (if debug logging enabled):
# - "GoodForBusiness" logs show spawning, hatching, feeding, abilities
# - Check for: "Registered X as network prefab", "Loaded X slime(s) from disk"

# Key log messages to verify:
# [GoodForBusiness] Registered Slime Egg as network prefab.
# [GoodForBusiness] Registered Slime Base as network prefab.
# [GoodForBusiness] Slime Manager singleton initialized.
# [GoodForBusiness] Host spawned egg at position
# [GoodForBusiness] Client received hatch notification
# [GoodForBusiness] Player X fed Y scrap to slime
# [GoodForBusiness] Slime grew to adult
# [GoodForBusiness] Equipment Slime spawned EquipmentCrate
# [GoodForBusiness] Bee Lover Slime spawned SlimedBeehive
```

### Post-Test Verification
- [ ] No errors in BepInEx console on any client
- [ ] No "NetworkObject not found" warnings
- [ ] No duplicate NetworkObjectId warnings
- [ ] Save file (`GoodForBusiness_slimes.json`) created and valid JSON
- [ ] Save file loads correctly on next session

## Known Limitations / Future Improvements

1. **NetworkTransform Interpolation**: Slime movement may appear slightly jittery on clients due to interpolation. Adjust `TeleportThreshold` if needed.

2. **Late Joiner Egg State**: Eggs in inventory/ship are not network-synced for late joiners (they're regular inventory items). This is a Lethal Company limitation.

3. **Config Hot-Reload**: Config changes require restart. CSync handles initial sync but not runtime changes.

4. **Bee Pathfinding**: Bee Lover Slime uses NavMeshAgent which may behave differently on clients vs host during ability. Teleport fallback mitigates this.

5. **Equipment Crate Auto-Pickup**: Only runs on host to prevent duplicate item granting. Clients see visual effect only.

## Troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| Slimes not visible to clients | NetworkPrefab not registered | Verify `RegisterNetworkPrefabs()` called |
| Duplicate slimes | Hatching runs on clients | Ensure `IsHost` check in `StartOfRoundPatch` |
| Growth not syncing | NetworkVariable not updating | Check `SyncGrowthProgressClientRpc` called |
| Items not spawning | NetworkObject missing | Verify prefab has `NetworkObject` component |
| JIP slimes missing | `RestoreSlimesFromSave` not called | Verify `RoundManagerLoadLevelPatch` runs |
| Config mismatch | CSync not working | Verify LethalLib dependency version |

## Acceptance Criteria (All Must Pass)

- [ ] **2 Players**: Full playthrough (egg find → hatch → feed → grow → ability) works
- [ ] **3 Players**: Same as above, no desyncs
- [ ] **4 Players**: Same as above, no performance issues
- [ ] **JIP**: Client joining mid-game sees correct state
- [ ] **Reconnection**: Client disconnect/reconnect preserves state
- [ ] **Save/Load**: Slimes persist across sessions
- [ ] **Config Sync**: All clients use host's config values
- [ ] **No Errors**: Clean BepInEx logs on all clients
- [ ] **Desync Free**: No position/state divergence > 0.5m or 1 frame

## Test Report Template

```
Test Session: [Date]
Players: [Host name] + [Client names]
Mod Version: [Version]
Game Version: [LC Version]

Results:
- 2 Player: PASS/FAIL - Notes:
- 3 Player: PASS/FAIL - Notes:
- 4 Player: PASS/FAIL - Notes:
- JIP: PASS/FAIL - Notes:
- Reconnection: PASS/FAIL - Notes:
- Save/Load: PASS/FAIL - Notes:
- Config Sync: PASS/FAIL - Notes:
- Errors: [List any errors]
- Desyncs: [List any desyncs observed]

Overall: PASS/FAIL
```