# Multiplayer Client-Side Optimization (MpClientOpt)

[![Nuclear Option](https://img.shields.io/badge/Game-Nuclear%20Option-blue)](https://store.steampowered.com/app/2168680/Nuclear_Option/)
[![Version](https://img.shields.io/badge/Version-v0.6.9-green)](https://github.com/Mursisru/MultiplayerClientSideOptimization/releases/tag/v0.6.9)
[![License](https://img.shields.io/badge/License-MIT-yellow)](LICENSE)

Client-side performance mod for **Nuclear Option** dedicated multiplayer clients. Trims **remoteSim** presentation cost (other players, missiles, mission units) without touching network authority, snapshots, or HUD/map logic.

**Stable release:** `v0.6.9` — full documentation refresh. Code matches `v0.6.8` (map icon sync, missile salvo, presentation physics fixes). FPS core unchanged from verified `v0.6.2`.

---

## Table of contents

- [Scope](#scope)
- [Requirements](#requirements)
- [Install](#install)
- [Upgrade](#upgrade)
- [Build](#build)
- [Configuration](#configuration-modini)
- [Distance tiers](#distance-tiers)
- [Combat presentation guards](#combat-presentation-guards)
- [What is optimized](#what-is-optimized-desync-safe)
- [Red line (never touched)](#red-line-never-touched)
- [DeepFreeze](#deepfreeze)
- [Troubleshooting](#troubleshooting)
- [Performance baseline](#performance-baseline)
- [Repository layout](#repository-layout)
- [Version history](#version-history)

---

## Scope

| In scope | Out of scope |
|----------|--------------|
| Dedicated server **client** (`Server.Active == false`) | Host / listen-server / headless server |
| `remoteSim` presentation shell | Local aircraft (`LocalSim`) |
| Distance-tiered visuals, components, RB apply | Network sync, snapshot **Insert**, Cmd/Rpc |
| DeepFreeze cosmetic cull (>40 km) | SyncVars, sync interval, `UpdateGridNow` |
| RAM reservoir + LOD/grass tuning | HUD / tactical map / ENGINE_TWEAKER budgets |

The mod **disables itself** when `NetworkManagerNuclearOption.i.Server.Active` is true.

---

## Requirements

- [Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/) (Steam)
- [NOLoader](https://github.com/at747/NOLoader_Engine) — primary loader
- Sibling repo for build/deploy: `../NOLoader_Engine`
- **First install only:** PatchTool must IL-patch `Assembly-CSharp.dll` (see [Install](#install))
- Optional: BepInEx 5 mirror build (see [Build](#build))

---

## Install

### Option A — ready-made zip (recommended)

1. Open [Release v0.6.9](https://github.com/Mursisru/MultiplayerClientSideOptimization/releases/tag/v0.6.9).
2. Download **`MpClientOpt-v0.6.9-NOLoader.zip`** (not *Source code*).
3. Extract into:

   `Nuclear Option\NOLoader\mods\MpClientOpt\`

4. **First install only** — close the game, then from a machine with `NOLoader_Engine` built:

```powershell
.\scripts\deploy-mp-opt-mod.ps1 -SkipPatchTool:$false
```

5. Join a **dedicated server** mission (not host).

### Option B — build from source

```powershell
.\scripts\deploy-mp-opt-mod.ps1
```

Install path: `Nuclear Option\NOLoader\mods\MpClientOpt\`

`deploy-mp-opt-mod.ps1` overwrites `mod.ini` in the game folder on each run.

---

## Upgrade

| From | To | PatchTool | Notes |
|------|-----|-----------|-------|
| Any `v0.6.2`–`v0.6.9` | New `v0.6.x` | **Not required** if IL patches already applied | Replace DLL + `mod.json` + `mod.ini` |
| Fresh install | `v0.6.x` | **Required once** | `-SkipPatchTool:$false` |
| `v0.6.3` / `v0.6.4` | `v0.6.5+` | Not required | Do **not** use reverted FPS-regression builds |

C#-only fixes (`v0.6.6`–`v0.6.8`) do not change the 58 IL patch manifest.

---

## Build

NOLoader (primary):

```powershell
dotnet build NOLoader.MultiplayerClientSideOptimization.csproj -c DEV_SDK
```

BepInEx mirror (run `.\scripts\fetch-bepinex-libs.ps1` once):

```powershell
dotnet build BepInEx\MultiplayerClientSideOptimization\MultiplayerClientSideOptimization.BepInEx.csproj -c Release
```

Output: `bin\DEV_SDK\net48\NOLoader.MultiplayerClientSideOptimization.dll`

---

## Configuration (`mod.ini`)

All keys under `[MpOpt]`. Shipped defaults:

```ini
[MpOpt]
optical_range_m=25000
optical_range_cap_m=12000
presentation_far_m=4000
presentation_near_m=800
presentation_full_m=400
fx_max_distance_m=1500
rb_move_throttle=1
rb_move_stride=4
rb_physics_sleep_m=3000
visual_update_stride=4
visual_full_offscreen_skip=1
visual_full_zone_stride=2
visual_mid_onscreen_stride=1
component_update_stride=3
component_full_offscreen_skip=1
memory_budget=1
memory_reservoir_mb=5120
asset_warm=1
texture_mipmap_limit=0
lod_bias_min=1.5
grass_position_buffer_percent=1
grass_visible_buffer_percent=0.5
deep_freeze=1
deep_freeze_min_m=40000
deep_freeze_scan_interval_s=1
deep_freeze_deopt_stride=2
deep_freeze_apply_per_scan=10
incoming_missile_max_m=80000
incoming_missile_dot_min=0.65
deep_freeze_disable_lights=1
deep_freeze_disable_renderers=0
```

### Key reference

| Key | Default | Purpose |
|-----|---------|---------|
| **Presentation zones** | | |
| `presentation_full_m` | 400 | Full-fidelity zone (m) |
| `presentation_near_m` | 800 | Near zone; component stride beyond full |
| `presentation_far_m` | 4000 | Combat zone — VisualUpdate never skipped inside |
| `fx_max_distance_m` | 1500 | Max distance for presentation VFX |
| **VisualUpdate budget** | | |
| `visual_update_stride` | 4 | Round-robin skip beyond near zone (not inside combat zone) |
| `visual_full_offscreen_skip` | 1 | Skip full-zone units when off-screen |
| `visual_full_zone_stride` | 2 | Stride inside full zone when on-screen |
| `visual_mid_onscreen_stride` | 1 | Extra skip in mid far band |
| **Component budget** | | |
| `component_update_stride` | 3 | Remote component FixedUpdate stride (mid zone) |
| `component_full_offscreen_skip` | 1 | Skip components in full zone when off-screen |
| **RB presentation** | | |
| `rb_move_throttle` | 1 | Transform-only `ApplySnapshot` for off-screen presentation units |
| `rb_move_stride` | 4 | Frame stride for RB throttle |
| `rb_physics_sleep_m` | 3000 | Never throttle RB inside this distance |
| **Missile optical** | | |
| `optical_range_m` | 25000 | Fallback optical range (m) |
| `optical_range_cap_m` | 12000 | Cap on detector-derived optical range |
| `incoming_missile_max_m` | 80000 | Incoming-missile exempt max range |
| `incoming_missile_dot_min` | 0.65 | Heading dot for incoming-missile exempt |
| **Memory / graphics** | | |
| `memory_budget` | 1 | Enable managed RAM reservoir |
| `memory_reservoir_mb` | **5120** | ~5 GB reservoir; reduces GC stutter |
| `asset_warm` | 1 | Hold encyclopedia prefab/material refs |
| `lod_bias_min` | 1.5 | LOD bias floor (restored on unload) |
| `texture_mipmap_limit` | 0 | Global mipmap limit clamp |
| `grass_position_buffer_percent` | 1 | Grass position buffer scale |
| `grass_visible_buffer_percent` | 0.5 | Grass visible buffer scale |
| **DeepFreeze** | | |
| `deep_freeze` | 1 | Enable >40 km freeze |
| `deep_freeze_min_m` | 40000 | Freeze distance threshold |
| `deep_freeze_scan_interval_s` | 1 | Scan period |
| `deep_freeze_deopt_stride` | 2 | Deopt frame stride |
| `deep_freeze_apply_per_scan` | 10 | Max units frozen per scan |
| `deep_freeze_disable_lights` | 1 | Disable lights when frozen |
| `deep_freeze_disable_renderers` | 0 | Disable mesh renderers when frozen |

Do **not** disable `memory_budget` on dedicated clients — `v0.6.1` showed significant FPS loss without the reservoir.

---

## Distance tiers

Observer distance drives presentation budget (local aircraft position, else camera):

```text
0 ───────── presentation_full_m (400m) ───────── presentation_near_m (800m)
      │ full zone (stride/offscreen)          │ near/mid (VisualUpdate stride)
      └──────────────── presentation_far_m (4000m) ─── combat zone ───┐
                                                                       │
                                              deep_freeze_min_m (40km) ┘
```

Beyond `presentation_far_m`, VisualUpdate and components may be stride-skipped. **Inside `presentation_far_m`**, combat guards keep transforms and missiles in sync (see below).

---

## Combat presentation guards

Prevents map/model/missile desync on dedicated clients (all remote units are `remoteSim`).

| Guard | Applies to | Effect |
|-------|------------|--------|
| `ShouldAlwaysRunVisualUpdate` | Aircraft, ships, GV, missiles | Never skip `NetworkTransform::VisualUpdate` within `presentation_far_m` or exempt paths |
| `ShouldAlwaysRunMissilePresentation` | Missiles | Never skip FU/motor; no kinematic sleep; vanilla `ApplySnapshot` |
| `ShouldSkipRemoteEngineComponent` | Turbofan, Turbojet, props, TurbineEngine | No engine skip at low LOD inside `presentation_far_m` |
| RB transform-only | Aircraft, GV, generic units | Always `rb.velocity` sync + `rb.Move` off-camera (vanilla semantics) |
| Own missiles | `MpPresentationExemptGuard` | Owner = local aircraft → exempt from cull/freeze |

Map icons use `TrackingInfo.GetPosition()` every frame; world models use `unit.transform` from `VisualUpdate`. Skipping VisualUpdate caused icon/model drift (fixed in `v0.6.8`).

---

## What is optimized (desync-safe)

| Tier | Targets |
|------|---------|
| **Green** | VFX shell (`VaporEffect`, `DownwashEffect`, `TurbineEngine`, particles) on remote / far units |
| **Green** | Remote aircraft components (gear, surfaces, pilot, fuel) — distance tiered |
| **Green** | Weapon aux shell (Gun, Laser, Radar, JammingPod, …) on presentation units |
| **Yellow** | `SendTransformBatcher::VisualUpdate` redirect + stride skip (outside combat zone) |
| **Yellow** | `SnapshotBuffer::RemoveOld` skip when VisualUpdate skipped |
| **Yellow** | Missile motor/FU skip only beyond optical **and** outside combat/exempt zone |
| **Yellow** | RB throttle — transform-only `ApplySnapshot` + velocity/`rb.Move` (missiles always vanilla) |
| **Runtime** | DeepFreeze + cosmetic cull >40 km |

**58 IL patches** in `mod.json`. Optimization core = verified stable `v0.6.2`.

---

## Red line (never touched)

- `SnapshotBuffer::Insert`, `*NetworkTransform::Receive`
- Cmd/Rpc, SyncVars, sync interval
- `UpdateGridNow`, HUD, tactical map, ENGINE_TWEAKER
- `Ship::ApplyJobResults` / physics jobs

---

## DeepFreeze

Presentation units beyond `deep_freeze_min_m` (40 km) are frozen: kinematic RB, behaviours/colliders/audio/particles disabled, lights optional off.

### Whitelist (never frozen)

- Distance ≤ `deep_freeze_min_m`
- In your target list
- Weapon or radar lock on you
- HQ tracking your aircraft
- Incoming missile (target ID or heading dot)
- **Own missiles** (owner = local aircraft)

On deopt: `MpDeepFreezeResync.ForceResyncTransform` restores position/velocity from NT snapshot buffer.

---

## Troubleshooting

| Symptom | Likely cause | Action |
|---------|--------------|--------|
| Map icon ≠ 3D model | Old build before `v0.6.8` | Upgrade to `v0.6.8+` |
| Missile salvo: sound but few visible | Old build before `v0.6.7` | Upgrade; ensure combat zone guards active |
| Afterburner on parked remote jet | Old build before `v0.6.6` | Upgrade |
| FPS drop after ini tweak | `memory_budget=0` | Restore `memory_budget=1` |
| Mod inactive | Hosting / listen server | Join dedicated client only |
| Patches not applied | Fresh install | Run PatchTool once |
| `v0.6.3` / `v0.6.4` | Known FPS regression | Use `v0.6.5+` only |

Logs: `Nuclear Option\NOLoader\logs\noloader_ring.log` — search `[MpOpt]`.

---

## Performance baseline

Verified on ~20-player dedicated client (`v0.6.2` / `v0.6.5` core):

| Scenario | FPS (approx.) |
|----------|----------------|
| Flight | ~40 |
| Hover | 45–55 |

`v0.6.3` and `v0.6.4` regressed FPS — **not shipped**. Do not re-enable their experimental ini/IL patterns without A/B testing.

---

## Repository layout

```text
src/
  MultiplayerClientSideOptimizationMod.cs   Entry (INOMod + tick)
  MpConfig.cs                               mod.ini loader
  MpSessionState.cs                         Active when client + not server
  MpPatchGuard.cs                           Presentation / optical / missile guards
  MpVisualBudget.cs                         Distance tiers + combat VisualUpdate guards
  MpPresentationExemptGuard.cs              Target/lock/incoming/own-missile exempt
  MpDeepFreezeManager.cs                    Scan / freeze / deopt
  MpDeepFreezeResync.cs                     Snapshot resync on thaw
  MpRbPresentationHelper.cs                 Transform-only RB apply (velocity + Move)
  MpMemoryBudget.cs                         RAM reservoir + graphics cache
  Patches/
    SendTransformBatcherRedirect.cs         VisualUpdate budget redirect
    NetworkVisualBudgetPatches.cs           Per-NT VisualUpdate prefix
    RbPresentationPatches.cs                  ApplySnapshot throttle
    MissileDistancePatches.cs               Missile optical skip + RB wake
    RemoteComponentPatches.cs               Engine/gear/surface stride
    PresentationVfxPatches.cs               VFX + ship propulsion
    SnapshotBufferPatches.cs                RemoveOld skip
    WeaponShellPatches.cs                   Gun/laser/radar shell
mod.json                                    58 IL patches (PatchTool)
mod.ini                                     Default configuration
scripts/deploy-mp-opt-mod.ps1               Build + copy to game (optional PatchTool)
BepInEx/                                    Harmony mirror (optional)
```

---

## Version history

See [CHANGELOG.md](CHANGELOG.md).

| Version | Highlight |
|---------|-----------|
| **0.6.9** | Full documentation refresh |
| **0.6.8** | Map icon vs model desync fix (#1) |
| **0.6.7** | Missile salvo visibility (dedicated client) |
| **0.6.6** | Afterburner, RB jitter, ship props, aircraft FU |
| **0.6.5** | Production freeze, 5 GB RAM, no diagnostics |
| **0.6.2** | Stable FPS baseline |

---

## License

MIT — see [LICENSE](LICENSE).
