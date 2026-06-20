# Multiplayer Client-Side Optimization (MpClientOpt)

[![Nuclear Option](https://img.shields.io/badge/Game-Nuclear%20Option-blue)](https://store.steampowered.com/app/2168680/Nuclear_Option/)
[![Version](https://img.shields.io/badge/Version-v0.6.5-green)](https://github.com/Mursisru/MultiplayerClientSideOptimization/releases/tag/v0.6.5)

Client-side performance mod for **Nuclear Option** dedicated multiplayer clients.

**Stable release:** `v0.6.5` — same optimization core as `v0.6.2`, no runtime diagnostics, 5 GB RAM reservoir.

---

## Scope

| In scope | Out of scope |
|----------|--------------|
| Dedicated server **client** (`Server.Active == false`) | Host / listen-server / headless server |
| Trim **remoteSim** presentation shell (other players, missiles, mission units) | Local aircraft (`LocalSim`) |
| Distance-tiered visuals, components, DeepFreeze (>40 km) | Network sync, snapshots, Cmd/Rpc |

The mod **disables itself** when `NetworkManagerNuclearOption.i.Server.Active` is true.

---

## Requirements

- [Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/) (Steam)
- [NOLoader](https://github.com/at747/NOLoader_Engine) (primary loader)
- Sibling repo: `../NOLoader_Engine`
- Optional: BepInEx 5 mirror build (see [Build](#build))

---

## Install

1. Close the game.
2. From this repository:

```powershell
.\scripts\deploy-mp-opt-mod.ps1
```

3. Install path: `Nuclear Option\NOLoader\mods\MpClientOpt\`
4. Join a **dedicated server** mission (not host).

If you already have `mod.ini` in the game folder, delete it or merge keys manually — deploy overwrites `mod.ini` on each run.

---

## Build

```powershell
dotnet build NOLoader.MultiplayerClientSideOptimization.csproj -c DEV_SDK
```

BepInEx mirror (run `.\scripts\fetch-bepinex-libs.ps1` once):

```powershell
dotnet build BepInEx\MultiplayerClientSideOptimization\MultiplayerClientSideOptimization.BepInEx.csproj -c Release
```

---

## Configuration (`mod.ini`)

All keys live under `[MpOpt]`. Defaults match the shipped `mod.ini`.

```ini
[MpOpt]
presentation_far_m=4000
presentation_near_m=800
presentation_full_m=400
visual_update_stride=4
component_update_stride=3
rb_move_throttle=1
memory_budget=1
memory_reservoir_mb=5120
asset_warm=1
lod_bias_min=1.5
grass_position_buffer_percent=1
deep_freeze=1
deep_freeze_min_m=40000
```

### Key settings

| Key | Default | Purpose |
|-----|---------|---------|
| `presentation_full_m` | 400 | Full-fidelity zone (m) |
| `presentation_near_m` | 800 | Near zone; stride applies beyond full |
| `presentation_far_m` | 4000 | Far skip tier |
| `visual_update_stride` | 4 | Round-robin `NetworkTransform::VisualUpdate` skip |
| `component_update_stride` | 3 | Remote component FixedUpdate stride (mid zone) |
| `rb_move_throttle` | 1 | Transform-only RB apply on presentation units |
| `memory_reservoir_mb` | **5120** | Managed RAM reservoir (~5 GB); reduces GC stutter |
| `memory_budget` | 1 | Enable reservoir + asset warm |
| `asset_warm` | 1 | Hold encyclopedia prefab/material refs |
| `lod_bias_min` | 1.5 | Clamp LOD bias (restored on unload) |
| `deep_freeze` | 1 | Freeze presentation units beyond 40 km |
| `deep_freeze_min_m` | 40000 | DeepFreeze distance threshold |

Do **not** disable `memory_budget` on dedicated clients — v0.6.1 showed significant FPS loss without the reservoir.

---

## What it optimizes (desync-safe)

| Tier | Targets |
|------|---------|
| **Green** | VFX shell (`VaporEffect`, `DownwashEffect`, `TurbineEngine`, particles) on remote / far units |
| **Green** | Remote aircraft components (engines, gear, surfaces, pilot) — distance tiered |
| **Green** | Weapon aux shell (Gun, Laser, Radar, JammingPod, …) on presentation units |
| **Yellow** | `NetworkTransform::VisualUpdate` + `SnapshotBuffer::RemoveOld` budget skip |
| **Yellow** | Optional RB throttle — transform-only `ApplySnapshot` |
| **Yellow** | Missile FixedUpdate / motor beyond optical range |
| **Runtime** | DeepFreeze + cosmetic cull >40 km (whitelist: target, lock, incoming missile) |

### Never touched (red line)

- `SnapshotBuffer::Insert`, `*NetworkTransform::Receive`
- Cmd/Rpc, SyncVars, sync interval, `UpdateGridNow`
- HUD / map / ENGINE_TWEAKER budgets
- Ship `ApplyJobResults` / physics jobs

**58 IL patches** (NOLoader `mod.json`). Behavior matches verified stable `v0.6.2`.

---

## DeepFreeze whitelist

Units are **never** frozen/culled when any of these is true:

- Distance ≤ `deep_freeze_min_m` (40 km)
- In your target list
- Weapon or radar lock on you
- Incoming missile toward you (heading check)

On deopt, position/velocity resync from NT snapshot buffer — no Insert/Cmd/Rpc.

---

## Repository layout

```text
src/MpVisualBudget.cs              Distance-tiered visual + component budget
src/MpDeepFreezeManager.cs         Scan/deopt manager (>40 km)
src/MpMemoryBudget.cs              RAM reservoir + graphics cache
src/Patches/SendTransformBatcherRedirect.cs
mod.json                           IL patch manifest (58 patches)
mod.ini                            Default configuration
scripts/deploy-mp-opt-mod.ps1      Build + deploy to game
```

---

## Version history

See [CHANGELOG.md](CHANGELOG.md).

---

## License

MIT — see [LICENSE](LICENSE).
