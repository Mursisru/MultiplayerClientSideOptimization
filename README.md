# Multiplayer Client-Side Optimization (Nuclear Option)



[![Nuclear Option](https://img.shields.io/badge/Game-Nuclear%20Option-blue)](https://store.steampowered.com/app/2168680/Nuclear_Option/)

[![Status](https://img.shields.io/badge/Status-v0.6.4-green)]()



Client-side performance mod for **Nuclear Option** multiplayer on **dedicated servers only** (pure client, never host).



**Version:** `0.6.4` (NOLoader primary, BepInEx mirror)



---



## Target scenario



| | |

|---|---|

| **In scope** | Dedicated server client → `Server.Active == false` → trim **remoteSim** presentation shell |

| **Out of scope** | Host, listen-server, headless server, `Server.Active == true` |



On dedicated client: **one** `LocalSim` aircraft (yours); all other aircraft, missiles, and mission units are presentation-only (`remoteSim`).



The mod **disables itself** when `NetworkManagerNuclearOption.i.Server.Active` is true.



---



## What it optimizes (desync-safe)



| Zone | Patches |

|------|---------|

| Green | `VaporEffect`, `DownwashEffect`, `TurbineEngine`, `ShipPropulsion` — skip on remote + low detail / far |

| Green | `Aircraft.FixedUpdate` — CM deploy kept; scrape/shake/debug path skipped on presentation units |

| Green | `Aircraft.ShakeAircraft`, `CheckSpawnedInPosition` (after first spawn) |

| Green | Remote aircraft components — engines, gear, props, FuelTank, RotorShaft, Pilot — **tiered** by distance |

| Green | **Turret**, **Gun**, **Laser**, **JammingPod**, **Radar**, **Repulsorlift**, **AirCushion**, **Transmission** on presentation units |

| Yellow | **Missile FixedUpdate** skip beyond optical (timeSinceSpawn preserved) |

| Yellow | `Missile.MotorThrust` — skip beyond dynamic optical range |

| Yellow | `NetworkTransform::VisualUpdate` budget + **`SnapshotBuffer::RemoveOld`** skip when visual skipped |

| Yellow | `ApplySnapshot` RB throttle — transform-only apply (optional, `rb_move_throttle=0` default) |

| Green | `ParticleEffectManager.EmitParticles`, `DamageParticles.Update` — distance cap |



**Never touched:** sync Hz, snapshot receive/buffer **Insert**, Cmd/Rpc, SyncVars, `UpdateGridNow`, ENGINE_TWEAKER HUD/map budgets.



---



## v0.3 highlights



- **RemoveOld skip:** when VisualUpdate is budget-skipped, `SnapshotBuffer.RemoveOld` is also skipped (fixes O(N) LateUpdate waste).

- **Tiered component shell:** full ≤600 m, stride 600–1000 m, skip >4 km / low detail.

- **Weapon/aux shell:** Gun/Laser/JammingPod/Radar linecast paths throttled on presentation units.

- **RAM cap:** `memory_reservoir_mb=4096` (~5.5 GB total process); do not raise above 4096 in v0.3.



### v0.3.1 (18-player FPS push)



- **SendTransformBatcher::VisualUpdate PrefixSkip:** custom loop with early-continue — distant transforms never enter `VisualUpdate`/`RemoveOld` (watch `batcher=` in profiler).

- **ControlSurface::UpdateJobFields** skip on presentation tier.

- **Stride in near zone:** units within `presentation_near_m` still round-robin skip via `visual_update_stride=3`.

- Defaults tightened: `presentation_near_m=1000`, `presentation_full_m=600`, `rb_move_throttle=1`.



### v0.4.0 — DeepFreeze (>40 km)



Runtime manager (no new IL patches) for presentation units **beyond 40 km**:



- **Slow scan (~1 Hz):** freeze eligible remote units (`rb.isKinematic`, disable sim behaviours, mute audio, stop particles, disable colliders/animators).

- **Fast deopt (every 2–3 frames):** instant restore when unit enters whitelist (≤40 km, your target, weapon/radar lock on you, incoming missile).

- **Resync on deopt:** position/velocity from NT snapshot buffer — no Insert/Cmd/Rpc.

- Works **alongside** v0.3.1 presentation shell; primary FPS gain in hot combat still from v0.3.x tiers.



**Whitelist (never freeze if any true):** dist ≤ `deep_freeze_min_m`, in your target list, holds lock/track on you, incoming missile toward you.



**Scope:** `UnitRegistry` only (aircraft, missiles, vehicles, ships). Gun `BulletSim` shells not included.



#### v0.4 test checklist



1. Dedicated client, busy mission — `[MpOpt] v0.4.0` with `frozen=` > 0 when units exist >40 km away.

2. Fly toward frozen unit — `deopt=` rises at ≤40 km, no visible teleport.

3. Select distant unit as target — it stays deopt while targeted.

4. Incoming missile on local aircraft — missile exempt from freeze.

5. CM deploy / optical missile track — manual desync smoke test.



### v0.6.0 — Deep Cosmetic Cull (>40 km)



Extends DeepFreeze with full cosmetic shutdown on the same whitelist (`MpPresentationExemptGuard`):

- **All non-network `Behaviour`** disabled (VaporEffect, DownwashEffect, DamageParticles, sim scripts, etc.)
- **Lights**, **TrailRenderer**, **LineRenderer**, legacy **Animation** snapshot-off
- **VisualUpdate skip** for frozen / deep non-exempt units (batch + NT patches)
- **VFX IL patches** aligned to `ShouldApplyDeepCull` at >40 km
- Optional `deep_freeze_disable_renderers=1` (pop-in risk on zoom)

**Whitelist (never cull if any true):** dist ≤ `deep_freeze_min_m`, your target, lock/track on you, incoming missile.

#### v0.6 test checklist

1. Dedicated client, units >40 km — `[MpOpt] v0.6.0` with `frozen=` > 0, `cosmeticBeh=` rising on scan.
2. Target distant unit — deopt, anim/particles/lights restore.
3. Incoming missile from >40 km — exempt, motor particles visible.
4. Fly toward frozen unit — deopt at ≤40 km, no stuck mesh.
5. Profiler: `visualDeep=` grows with frozen count; `cosmeticLights=` on missile/aircraft freeze.

### v0.6.4 — Mid-zone component budget (flight FPS)

Fixes the v0.6.2 blind spot: **VisualUpdate** was strided in 800–4000 m, but **remote components** (engines, gear, surfaces) still ran every frame.

- **`component_mid_onscreen_stride=1`** — mirrors `visual_mid_onscreen_stride` for the 800–4000 m band (no new IL on hot `Update` paths).
- **`MpMotionBudget`** — when local speed ≥ `motion_speed_threshold_mps` (80), scales presentation full/near zones by `motion_zone_scale` (0.85) for skip decisions only.
- **`JetNozzle::SlowUpdate`** — far/low-detail skip (same pattern as `TurbineEngine`), 59 IL patches total.
- **Not included (v0.6.3 lesson):** `GroundVehicle`/`FlareEjector`/`ChaffEjector` Update IL, `grass_position_buffer_percent < 1`.

Profiler: watch `compMid=` and `jetNozzle=`; rollback `component_mid_onscreen_stride=0` if gear/surface stutter.

#### v0.6.4 test checklist

1. Full restart, dedicated ~20p — `[MpOpt] v0.6.4` with `compMid=` rising in flight.
2. Flight vs v0.6.2 baseline (~40 FPS) — target **43–45 FPS**; hover ≥45.
3. If FPS drops — set `motion_stride_enabled=0` or `component_mid_onscreen_stride=0` and retest.
4. Optional ini micro (only if still short): `presentation_near_m=750`, `presentation_full_m=350`, `fx_max_distance_m=1300`, `visual_update_stride=5` — **one key at a time**.



---



## Requirements



- [Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/) (Steam)

- [NOLoader](https://github.com/at747/NOLoader_Engine) (primary loader)

- Sibling repo: `../NOLoader_Engine`

- Optional: BepInEx 5 in game folder for runtime deploy of the BepInEx build



---



## Build



```powershell

dotnet build NOLoader.MultiplayerClientSideOptimization.csproj -c DEV_SDK

```



BepInEx mirror (needs `BepInEx/lib/` — run once):



```powershell

.\scripts\fetch-bepinex-libs.ps1

dotnet build BepInEx\MultiplayerClientSideOptimization\MultiplayerClientSideOptimization.BepInEx.csproj -c Release

```



---



## Deploy (NOLoader)



Close the game, then:



```powershell

.\scripts\deploy-mp-opt-mod.ps1

```



Install path: `Nuclear Option\NOLoader\mods\MpClientOpt\`



After deploy, set `mod.ini` keys if an older ini exists (deploy only copies ini on first install).



---



## Configuration (`mod.ini`)



```ini

[MpOpt]

profiler=1

report_interval_s=5

optical_range_m=25000

optical_range_cap_m=12000

presentation_far_m=4000

presentation_near_m=1000

presentation_full_m=600

fx_max_distance_m=2000

rb_move_throttle=1

rb_move_stride=4

rb_physics_sleep_m=5000

visual_update_stride=3

visual_mid_onscreen_stride=1

component_update_stride=2

memory_budget=1

memory_reservoir_mb=4096

asset_warm=1

texture_mipmap_limit=0

lod_bias_min=1.5

grass_position_buffer_percent=1

grass_visible_buffer_percent=0.5

deep_freeze=1

deep_freeze_min_m=40000

deep_freeze_scan_interval_s=1

deep_freeze_deopt_stride=2

incoming_missile_max_m=80000

incoming_missile_dot_min=0.65

deep_freeze_disable_lights=1

deep_freeze_disable_renderers=0

```



| Key | Default | Notes |

|-----|---------|-------|

| `presentation_full_m` | 600 | Full component/visual fidelity zone |

| `presentation_near_m` | 1000 | Near zone; VisualUpdate still stride-skipped |

| `presentation_far_m` | 4000 | Far skip tier |

| `component_update_stride` | 2 | Mid-zone (full..near) component FU stride |

| `visual_update_stride` | 3 | Round-robin VisualUpdate skip (includes near zone) |

| `rb_move_throttle` | 1 | Transform-only RB apply on presentation units |

| `memory_reservoir_mb` | **4096** | RAM cap v0.3 (~5.5 GB total process) |

| `deep_freeze` | 1 | Enable DeepFreeze manager (>40 km) |

| `deep_freeze_min_m` | 40000 | Freeze only beyond this distance |

| `deep_freeze_deopt_stride` | 2 | Frames between fast deopt checks |

| `incoming_missile_max_m` | 80000 | Heading-based incoming missile exempt range |

| `deep_freeze_disable_lights` | 1 | Disable Light components on freeze |

| `deep_freeze_disable_renderers` | 0 | Disable MeshRenderer/SkinnedMeshRenderer (opt-in) |



---



## Profiler



Every `report_interval_s`, NOLoader ring log:



```text

[MpOpt] v0.6.0 units=… lateMs=… fixedMs=… visual=… removeOld=… batcher=… frozen=… visualDeep=… cosmeticBeh=… reservedMb=4096

```



Watch `batcher=`, `frozen=`, `visualDeep=`, `removeOld=` on busy missions — confirms presentation shell + DeepFreeze cosmetic cull are active.



---



## Repository layout



```text

src/MpBufferUnitMap.cs           SnapshotBuffer → Unit map for RemoveOld skip

src/MpVisualBudget.cs            Visual + component tiered budget

src/Patches/SnapshotBufferPatches.cs

src/MpPresentationExemptGuard.cs   Unified whitelist (target/lock/incoming missile)

src/MpDeepFreezeManager.cs         DeepFreeze scan/deopt (>40 km)

src/MpDeepFreezeState.cs           Snapshot apply/restore + cosmetic cull

src/Patches/ControlSurfaceJobPatches.cs

mod.json                         45 IL patches (unchanged v0.4)

```



---



## License



MIT — see [LICENSE](LICENSE).

