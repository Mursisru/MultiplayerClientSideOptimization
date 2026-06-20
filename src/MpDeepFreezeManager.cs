using System.Collections.Generic;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpDeepFreezeManager
    {
        private static readonly Dictionary<PersistentID, MpDeepFreezeState> Frozen =
            new Dictionary<PersistentID, MpDeepFreezeState>();

        private static readonly List<PersistentID> DeoptKeys = new List<PersistentID>(64);
        private static float _scanAccumulator;
        private static int _deoptFrameCounter;

        internal static int FrozenCount => Frozen.Count;

        internal static bool IsFrozenId(PersistentID id) =>
            id.IsValid && Frozen.ContainsKey(id);

        internal static bool IsFrozen(Unit? unit) =>
            unit != null && IsFrozenId(unit.persistentID);

        internal static void TickScan(float dt)
        {
            if (!MpConfig.DeepFreezeEnabled || !MpSessionState.Active)
                return;

            _scanAccumulator += dt;
            if (_scanAccumulator < MpConfig.DeepFreezeScanIntervalS)
                return;

            _scanAccumulator = 0f;

            if (!GameManager.GetLocalAircraft(out Aircraft local) || local == null)
                return;

            int appliedThisScan = 0;
            int applyLimit = MpConfig.DeepFreezeApplyPerScan;

            foreach (Unit unit in UnitRegistry.allUnits)
            {
                if (appliedThisScan >= applyLimit)
                    break;

                if (unit == null)
                    continue;

                PersistentID id = unit.persistentID;
                if (!id.IsValid)
                    continue;

                if (Frozen.ContainsKey(id))
                    continue;

                if (!MpDeepFreezeGuard.ShouldApplyFreeze(unit, local))
                    continue;

                var state = new MpDeepFreezeState();
                if (!state.Apply(unit))
                    continue;

                Frozen[id] = state;
                appliedThisScan++;
            }
        }

        internal static void TickDeopt()
        {
            if (!MpConfig.DeepFreezeEnabled || !MpSessionState.Active)
                return;

            if (Frozen.Count == 0)
                return;

            _deoptFrameCounter++;
            if (_deoptFrameCounter % MpConfig.DeepFreezeDeoptStride != 0)
                return;

            if (!GameManager.GetLocalAircraft(out Aircraft local) || local == null)
                return;

            DeoptKeys.Clear();
            foreach (KeyValuePair<PersistentID, MpDeepFreezeState> pair in Frozen)
                DeoptKeys.Add(pair.Key);

            for (int i = 0; i < DeoptKeys.Count; i++)
            {
                PersistentID id = DeoptKeys[i];
                if (!Frozen.TryGetValue(id, out MpDeepFreezeState? state))
                    continue;

                if (!UnitRegistry.TryGetUnit(new PersistentID?(id), out Unit? unit) || unit == null)
                {
                    Frozen.Remove(id);
                    continue;
                }

                if (MpDeepFreezeGuard.ShouldExemptFromFreeze(unit, local)
                    || !MpDeepFreezeGuard.IsEligibleCandidate(unit))
                {
                    state.Restore(unit, resync: true);
                    Frozen.Remove(id);
                }
            }
        }

        internal static void RestoreAll()
        {
            if (Frozen.Count == 0)
                return;

            DeoptKeys.Clear();
            foreach (KeyValuePair<PersistentID, MpDeepFreezeState> pair in Frozen)
                DeoptKeys.Add(pair.Key);

            for (int i = 0; i < DeoptKeys.Count; i++)
            {
                PersistentID id = DeoptKeys[i];
                if (!Frozen.TryGetValue(id, out MpDeepFreezeState? state))
                    continue;

                if (UnitRegistry.TryGetUnit(new PersistentID?(id), out Unit? unit) && unit != null)
                    state.Restore(unit, resync: true);

                Frozen.Remove(id);
            }
        }
    }
}
