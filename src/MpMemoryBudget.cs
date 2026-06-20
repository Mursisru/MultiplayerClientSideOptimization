using System.Collections.Generic;
using NuclearOption.Effects;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpMemoryBudget
    {
        private static readonly List<byte[]> Reservoir = new List<byte[]>();
        private static readonly List<Object> WarmRefs = new List<Object>();
        private static bool _applied;
        private static int _reservedMb;

        internal static int ReservedMb => _reservedMb;

        internal static void TryApplyOnce()
        {
            if (_applied || !MpSessionState.Active || !MpConfig.MemoryBudgetEnabled)
                return;

            // #region agent log
            float lodBefore = QualitySettings.lodBias;
            int mipBefore = QualitySettings.globalTextureMipmapLimit;
            // #endregion

            ApplyReservoir();
            ApplyGraphicsCache();
            TryWarmAssets();
            _applied = true;

            // #region agent log
            MpDebugTrace.Log(
                "H1",
                "MpMemoryBudget.cs:TryApplyOnce",
                "memory budget applied",
                "{\"lodBiasBefore\":" + MpDebugTrace.F(lodBefore)
                + ",\"lodBiasAfter\":" + MpDebugTrace.F(QualitySettings.lodBias)
                + ",\"mipmapBefore\":" + MpDebugTrace.I(mipBefore)
                + ",\"mipmapAfter\":" + MpDebugTrace.I(QualitySettings.globalTextureMipmapLimit)
                + ",\"lodBiasMinCfg\":" + MpDebugTrace.F(MpConfig.LodBiasMin)
                + ",\"textureMipmapCfg\":" + MpDebugTrace.I(MpConfig.TextureMipmapLimit)
                + ",\"reservedMb\":" + MpDebugTrace.I(_reservedMb)
                + ",\"warmRefs\":" + MpDebugTrace.I(WarmRefs.Count)
                + ",\"monoMb\":" + MpDebugTrace.I((int)GetMonoUsedMb()) + "}");
            // #endregion

            Debug.Log("[MpOpt] memory budget reservedMb=" + _reservedMb
                + " warmRefs=" + WarmRefs.Count
                + " monoMb=" + GetMonoUsedMb());
        }

        internal static void Release()
        {
            Reservoir.Clear();
            WarmRefs.Clear();
            _applied = false;
            _reservedMb = 0;
        }

        private static void ApplyReservoir()
        {
            if (Reservoir.Count > 0)
                return;

            int targetMb = MpConfig.MemoryReservoirMb;
            if (targetMb <= 0)
                return;

            const int chunkMb = 64;
            int chunks = targetMb / chunkMb;
            if (chunks <= 0)
                chunks = 1;

            for (int i = 0; i < chunks; i++)
            {
                byte[] chunk = new byte[chunkMb * 1024 * 1024];
                chunk[0] = 1;
                chunk[chunk.Length - 1] = 2;
                Reservoir.Add(chunk);
            }

            _reservedMb = chunks * chunkMb;
        }

        private static void ApplyGraphicsCache()
        {
            if (MpConfig.TextureMipmapLimit >= 0)
                QualitySettings.globalTextureMipmapLimit = MpConfig.TextureMipmapLimit;

            if (MpConfig.LodBiasMin > 0f && QualitySettings.lodBias < MpConfig.LodBiasMin)
                QualitySettings.lodBias = MpConfig.LodBiasMin;

            TryBoostGrassBuffers();
        }

        private static void TryBoostGrassBuffers()
        {
            if (MpConfig.GrassPositionBufferPercent <= 0f && MpConfig.GrassVisibleBufferPercent <= 0f)
                return;

            GrassRenderer[] renderers = Object.FindObjectsOfType<GrassRenderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                GrassRenderer grass = renderers[i];
                if (grass == null)
                    continue;

                if (MpConfig.GrassPositionBufferPercent > 0f)
                    grass.PositionBufferSizePercent = MpConfig.GrassPositionBufferPercent;

                if (MpConfig.GrassVisibleBufferPercent > 0f)
                    grass.VisibleBufferSizePercent = MpConfig.GrassVisibleBufferPercent;
            }
        }

        private static void TryWarmAssets()
        {
            if (!MpConfig.AssetWarmEnabled || WarmRefs.Count > 0)
                return;

            try
            {
                Encyclopedia enc = Encyclopedia.i;
                WarmDefinitions(enc.aircraft);
                WarmDefinitions(enc.vehicles);
                WarmDefinitions(enc.missiles);
                WarmDefinitions(enc.ships);
                WarmDefinitions(enc.buildings);
                WarmDefinitions(enc.otherUnits);
                WarmDefinitions(enc.scenery);
            }
            catch
            {
            }
        }

        private static void WarmDefinitions<T>(List<T>? definitions) where T : UnitDefinition
        {
            if (definitions == null)
                return;

            for (int i = 0; i < definitions.Count; i++)
            {
                UnitDefinition def = definitions[i];
                if (def == null)
                    continue;

                GameObject? prefab = def.unitPrefab;
                if (prefab == null)
                    continue;

                WarmRefs.Add(prefab);
                Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
                for (int r = 0; r < renderers.Length; r++)
                {
                    Renderer renderer = renderers[r];
                    if (renderer == null)
                        continue;

                    Material[] materials = renderer.sharedMaterials;
                    for (int m = 0; m < materials.Length; m++)
                    {
                        Material? mat = materials[m];
                        if (mat != null)
                            WarmRefs.Add(mat);
                    }
                }
            }
        }

        internal static long GetMonoUsedMb()
        {
            return UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong() / (1024 * 1024);
        }
    }
}
