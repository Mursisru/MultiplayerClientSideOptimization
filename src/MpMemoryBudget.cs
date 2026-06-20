using System.Collections.Generic;
using NuclearOption.Effects;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpMemoryBudget
    {
        private static readonly List<byte[]> Reservoir = new List<byte[]>();
        private static readonly List<Object> WarmRefs = new List<Object>();
        private static bool _memoryApplied;
        private static bool _graphicsCaptured;
        private static float _savedLodBias = -1f;
        private static int _savedMipmapLimit = -1;
        private static int _reservedMb;

        internal static void ApplyGraphicsSettings()
        {
            if (!MpSessionState.Active)
                return;

            if (!_graphicsCaptured)
            {
                _savedLodBias = QualitySettings.lodBias;
                _savedMipmapLimit = QualitySettings.globalTextureMipmapLimit;
                _graphicsCaptured = true;
            }

            ApplyGraphicsCache();
        }

        internal static void TryApplyOnce()
        {
            if (_memoryApplied || !MpSessionState.Active || !MpConfig.MemoryBudgetEnabled)
                return;

            ApplyReservoir();
            TryWarmAssets();
            _memoryApplied = true;
        }

        internal static void Release()
        {
            RestoreGraphicsSettings();
            Reservoir.Clear();
            WarmRefs.Clear();
            _memoryApplied = false;
            _reservedMb = 0;
        }

        private static void RestoreGraphicsSettings()
        {
            if (!_graphicsCaptured)
                return;

            QualitySettings.lodBias = _savedLodBias;
            QualitySettings.globalTextureMipmapLimit = _savedMipmapLimit;
            _graphicsCaptured = false;
            _savedLodBias = -1f;
            _savedMipmapLimit = -1;
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

            if (MpConfig.LodBiasMin > 0f)
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
    }
}
