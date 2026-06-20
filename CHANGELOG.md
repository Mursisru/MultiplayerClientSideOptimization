# Changelog

All notable changes to MpClientOpt (Multiplayer Client-Side Optimization).

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
