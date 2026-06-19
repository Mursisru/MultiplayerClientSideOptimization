# MultiplayerClientSideOptimization (Nuclear Option Mod)

[![Nuclear Option](https://img.shields.io/badge/Game-Nuclear%20Option-blue)](https://store.steampowered.com/app/2168680/Nuclear_Option/)
[![Status](https://img.shields.io/badge/Status-Pre--alpha-lightgrey)]()

Client-side performance mod for **Nuclear Option** multiplayer. Goal: reduce FPS drops in large MP sessions (often down to ~20 FPS) through targeted client optimizations.

> **Early stage:** repository scaffold only. Implementation has not started yet.

---

## Problem

In multiplayer, client FPS can collapse under load — many units, effects, and network-driven updates competing for the same frame budget. This project will explore **client-side** optimizations that do not change server authority or gameplay outcomes.

---

## Planned scope (draft)

* Profiling hooks and baseline metrics in MP scenarios
* Reducing per-frame work (rendering, UI, audio, particle/effect budgets)
* Throttling or LOD for non-critical client visuals
* Configurable quality presets for MP vs SP

Loader target (BepInEx / NOLoader) will be chosen during implementation.

---

## Requirements

* **Nuclear Option** ([Steam](https://store.steampowered.com/app/2168680/Nuclear_Option/))
* Mod loader — TBD

---

## Licence

MIT License — see [LICENSE](LICENSE).
