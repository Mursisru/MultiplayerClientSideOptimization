# Changelog

All notable changes to MpClientOpt (Multiplayer Client-Side Optimization).

## [0.6.9] — 2026-06-30

**Documentation.** No code or IL patch changes from `v0.6.8`.

### Changed

- Full `README.md` rewrite: install/upgrade, complete `mod.ini` reference, distance tiers, combat presentation guards, troubleshooting, performance baseline, repository layout.
- Version table and upgrade matrix (PatchTool required only on first install).

---

## [0.6.8] — 2026-06-25

**Map icon / model desync (GitHub #1).** Map icons use `TrackingInfo` every frame; throttled `VisualUpdate` left `unit.transform` stale.

### Fixed

- `ShouldAlwaysRunVisualUpdate`: presentation aircraft/ships/GV/missiles within `presentation_far_m` (and exempt/target paths) never skip `NetworkTransform::VisualUpdate`.
- RB transform-only path now always calls `rb.Move` when off-camera (matches vanilla `ApplySnapshot`); position no longer freezes while velocity syncs.

---

## [0.6.7] — 2026-06-19

**Missile salvo / visibility fix.** On dedicated clients every missile is `remoteSim`; v0.6.6 still throttled them via kinematic sleep + `VisualUpdate` budget.

### Fixed

- Removed missile kinematic sleep entirely (vanilla `VisualUpdate` skips `ApplySnapshot` when `rb.isKinematic`).
- `ShouldAlwaysRunMissilePresentation`: own missiles, incoming/exempt missiles, and all missiles within `presentation_far_m` never skip `VisualUpdate`, `FixedUpdate`, or `MotorThrust`.
- `EnsureMissileRigidbodyAwake` clears stale kinematic state on presentation missiles in the combat zone.
- Own missiles added to presentation exempt guard (DeepFreeze / deep cull whitelist).

### Unchanged

- 58 IL patches, aircraft/ship/engine fixes from `v0.6.6`, FPS core.

---

## [0.6.6] — 2026-06-19

**Presentation physics / VFX correctness.** Same 58 IL patches and FPS core as `v0.6.5`; C# logic fixes only (no PatchTool re-run).

### Fixed

- **Missiles:** wake kinematic RB when entering optical/on-screen range; always use vanilla `MissileNetworkTransform.ApplySnapshot` (no transform-only throttle).
- **Afterburner on apron:** engine components (Turbofan/Turbojet/PropFan/ConstantSpeedProp/TurbineEngine) no longer skipped at low LOD within `presentation_far_m`.
- **Physics jitter:** transform-only RB apply now syncs `rb.velocity` / `angularVelocity`; restored vanilla `Aircraft::FixedUpdate` for presentation units.
- **Ship propellers:** fixed inverted `ShipPropulsion::FixedUpdate` skip on remote ships.

### Unchanged

- Optimization tiers, DeepFreeze, memory reservoir (5120 MB), no runtime diagnostics.

---

## [0.6.5] — 2026-06-20

**Stable production release.** Optimization core unchanged from verified `v0.6.2`.

### Changed

- Removed all runtime diagnostics: profiler, skip counters, periodic LoaderLog/BepInEx stats, `Debug.Log` memory reports.
- Increased default `memory_reservoir_mb` from 4096 to **5120** (~5 GB managed reservoir).
- Documentation rewrite (`README.md`, this file).

### Unchanged

- 58 IL patches and presentation/DeepFreeze behavior from `v0.6.2`.
- Verified baseline (~40 FPS in flight, 45–55 hover on ~20p dedicated client).

### Not shipped (regressed in testing)

- `v0.6.3` — bundled ini + motion + per-frame Update IL patches (−5 FPS).
- `v0.6.4` — mid-zone component stride + motion + JetNozzle (−FPS).

---

## [0.6.2] — 2026-06-19

- Restore `memory_budget=1` and `asset_warm=1` (fixes v0.6.1 FPS regression).
- Fix LOD bias apply/restore (`lod_bias_min=1.5`).
- DeepFreeze apply budget (`deep_freeze_apply_per_scan=10`).

## [0.6.0] — 2026-06-19

- Deep cosmetic cull (>40 km) with unified exempt guard.
- VisualUpdate skip for frozen/deep units.

## [0.4.0] — 2026-06-19

- DeepFreeze manager (>40 km): kinematic RB, disable sim behaviours, resync on deopt.

## [0.3.1] — 2026-06-19

- SendTransformBatcher VisualUpdate redirect with early-continue.
- SnapshotBuffer RemoveOld skip when visual skipped.

## [0.3.0] — 2026-06-19

- Tiered presentation shell, weapon aux throttling, RAM reservoir.
