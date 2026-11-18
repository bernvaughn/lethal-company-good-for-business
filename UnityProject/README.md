# GoodForBusiness Unity Project

Unity 2022.3.9f1 project for building the GoodForBusiness asset bundle.

## Project Structure

```
UnityProject/
├── Assets/
│   ├── Models/          # 3D models (FBX) - Prefix: SM_
│   ├── Materials/       # Materials & Textures - Prefix: M_, T_
│   ├── Animations/      # Animation clips - Prefix: ANIM_
│   ├── Audio/           # Audio clips - Prefix: SFX_
│   ├── Prefabs/         # Prefabs - Prefix: BP_
│   ├── Items/           # LethalLib ScriptableObjects (Item, EnemyType) - Prefix: ITEM_, ENEMY_
│   ├── Scripts/
│   │   ├── Runtime/     # Runtime scripts (compiled into plugin)
│   │   └── Editor/      # Editor scripts (build pipeline, validation)
│   └── Editor/          # Legacy editor scripts location
├── Packages/
│   └── manifest.json    # Unity package dependencies
├── ProjectSettings/     # Unity project settings
└── Build/               # Build output (gitignored)
```

## Required Unity Version

**Unity 2022.3.9f1** (LTS) - Must match Lethal Company's Unity version exactly.

## Required Packages

- **Asset Bundle Browser** (com.unity.assetbundlebrowser) 2.2.0 - For inspecting bundles
- **Newtonsoft JSON** (com.unity.nuget.newtonsoft-json) 3.2.1 - For JSON serialization

## Asset Bundle Configuration

- **Bundle Name**: `gfbassets`
- **Target Platform**: `StandaloneWindows64`
- **Compression**: `ChunkBasedCompression` (LZ4)
- **Build Options**: `StrictMode`

## Building Asset Bundles

### Via Unity Editor (Menu)

1. Open project in Unity 2022.3.9f1
2. Use menu: **GoodForBusiness > Build Asset Bundles (Development)** or **GoodForBusiness > Build Asset Bundles (Release)**
3. Output: `UnityProject/Build/AssetBundles/StandaloneWindows64/gfbassets`
4. Automatically copied to: `../GoodForBusiness/Assets/StreamingAssets/gfbassets`

### Via Command Line

```bash
# Windows
"C:\Program Files\Unity\Hub\Editor\2022.3.9f1\Editor\Unity.exe" -batchmode -projectPath "UnityProject" -executeMethod GoodForBusiness.Editor.AssetBundleBuilder.BuildAssetBundlesRelease -quit

# Linux
/opt/unity/2022.3.9f1/Editor/Unity -batchmode -projectPath "UnityProject" -executeMethod GoodForBusiness.Editor.AssetBundleBuilder.BuildAssetBundlesRelease -quit
```

## Asset Naming Conventions

| Asset Type | Prefix | Folder | Example |
|------------|--------|--------|---------|
| Model (FBX) | `SM_` | `Assets/Models/` | `SM_SlimeEgg.fbx` |
| Prefab | `BP_` | `Assets/Prefabs/` | `BP_SlimeEgg.prefab` |
| Material | `M_` | `Assets/Materials/` | `M_SlimeBlue.mat` |
| Texture | `T_` | `Assets/Materials/` | `T_SlimeBlue_Normal.png` |
| Animation | `ANIM_` | `Assets/Animations/` | `ANIM_Slime_Idle.anim` |
| Audio | `SFX_` | `Assets/Audio/` | `SFX_Slime_Hatch.wav` |
| Item Asset | `ITEM_` | `Assets/Items/` | `ITEM_SlimeEgg.asset` |
| EnemyType Asset | `ENEMY_` | `Assets/Items/` | `ENEMY_SlimeBase.asset` |

### Validation

Use menu: **GoodForBusiness > Validate Asset Naming** to check all assets follow conventions.

## Expected Assets

### Models
- `SM_SlimeEgg.fbx` - Slime egg pickup model
- `SM_SlimeBase.fbx` - Base slime model (soccer ball size)
- `SM_EquipmentCrate.fbx` - Equipment crate model
- `SM_SlimedBeehive.fbx` - Slimed beehive model

### Prefabs
- `BP_SlimeEgg.prefab` - GrabbableObject for egg
- `BP_SlimeBase.prefab` - Base EnemyAI prefab
- `BP_SlimeEquipment.prefab` - Equipment slime variant (blue)
- `BP_SlimeBeeLover.prefab` - Bee lover slime variant (yellow/black)
- `BP_EquipmentCrate.prefab` - Spawned equipment container
- `BP_SlimedBeehive.prefab` - Sellable scrap prefab

### Materials
- `M_SlimeEgg.mat` - Egg material
- `M_SlimeBlue.mat` - Equipment slime material
- `M_SlimeYellowBlack.mat` - Bee lover slime material
- `M_SlimedBeehive.mat` - Slimed beehive material
- `M_EquipmentCrate.mat` - Equipment crate material

### Animations
- `ANIM_Slime_Hatch.anim` - Hatching animation
- `ANIM_Slime_Idle.anim` - Idle/bob animation
- `ANIM_Slime_Eat.anim` - Eating animation
- `ANIM_Slime_Grow.anim` - Growth animation
- `ANIM_Slime_Ability.anim` - Ability activation animation

### Audio
- `SFX_Slime_Hatch.wav` - Hatch sound
- `SFX_Slime_Eat.wav` - Eating sound
- `SFX_Slime_Grow.wav` - Growth sound
- `SFX_Slime_Ability.wav` - Ability sound

### LethalLib Assets (ScriptableObjects)
- `ITEM_SlimeEgg.asset` - Scrap item definition
- `ENEMY_SlimeBase.asset` - EnemyType definition
- `ITEM_SlimedBeehive.asset` - Slimed beehive scrap
- `ITEM_EquipmentCrate.asset` - Equipment crate item

## Integration with Plugin

The plugin loads the asset bundle at startup via `AssetBundleLoader.LoadBundle(pluginPath)`.

Assets are retrieved using:
```csharp
var prefab = AssetBundleLoader.LoadAsset<GameObject>("BP_SlimeEgg");
var material = AssetBundleLoader.LoadAsset<Material>("M_SlimeBlue");
var itemAsset = AssetBundleLoader.LoadAsset<Item>("ITEM_SlimeEgg");
```

## LethalLib Registration

Assets are registered with LethalLib at runtime:

```csharp
// Items (Scrap)
Items.RegisterScrap(itemAsset, spawnPrefab, rarityConfig);

// Enemies
Enemies.RegisterEnemy(enemyTypeAsset, enemyPrefab, spawnConfig);
```

## Troubleshooting

### Bundle not loading
- Verify bundle exists at `BepInEx/plugins/GoodForBusiness/Assets/StreamingAssets/gfbassets`
- Check Unity version matches exactly (2022.3.9f1)
- Verify bundle was built for `StandaloneWindows64`

### Missing assets in bundle
- Run **GoodForBusiness > Apply Asset Bundle Labels**
- Verify assets are in watched folders
- Check **GoodForBusiness > Validate Asset Bundle Setup**

### Shader errors
- Use Unity Standard Shader or compatible URP shaders
- Lethal Company uses Built-in Render Pipeline
- Avoid URP/HDRP specific features

### Network sync issues
- All NetworkObject prefabs must be in the bundle
- Prefabs must have NetworkObject component
- Register prefabs via `NetworkManager.Singleton.AddNetworkPrefab()`