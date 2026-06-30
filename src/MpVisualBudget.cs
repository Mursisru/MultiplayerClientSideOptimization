using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpVisualBudget
    {
        internal static bool ShouldSkipVisualUpdate(Unit? unit, Vector3 worldPosition)
        {
            if (!MpSessionState.Active || unit == null)
                return false;

            if (unit is Missile missile && ShouldAlwaysRunMissilePresentation(missile))
                return false;

            if (ShouldAlwaysRunVisualUpdate(unit))
                return false;

            if (unit.LocalSim || GameManager.IsLocalAircraft(unit))
                return false;

            if (!MpPatchGuard.IsPresentationUnit(unit))
                return false;

            if (ShouldSkipDeepVisualUpdate(unit))
                return true;

            if (MpPatchGuard.IsLocalSelectedTarget(unit))
                return false;

            float dist = MpPatchGuard.DistanceToObserver(unit);

            if (dist <= MpConfig.PresentationFullM)
                return ShouldSkipFullZoneVisual(unit, worldPosition);

            int stride = MpConfig.VisualUpdateStride;
            int slot = GetUnitSlot(unit);

            if (dist <= MpConfig.PresentationNearM)
            {
                if (stride <= 1)
                    return false;
                return Time.frameCount % stride != slot % stride;
            }

            if (stride <= 1)
            {
                if (dist <= MpConfig.PresentationFarM)
                    return false;

                return MpPatchGuard.IsOffScreen(worldPosition);
            }

            if (Time.frameCount % stride != slot % stride)
                return false;

            if (dist > MpConfig.PresentationFarM)
                return true;

            if (MpConfig.VisualMidOnscreenStride)
                return true;

            return MpPatchGuard.IsOffScreen(worldPosition);
        }

        internal static bool ShouldSkipPresentationComponent(Unit? unit)
        {
            if (unit == null || !MpSessionState.Active)
                return false;

            if (!MpPatchGuard.IsPresentationUnit(unit))
                return false;

            if (ShouldSkipDeepVisualUpdate(unit))
                return true;

            if (MpPatchGuard.IsLocalSelectedTarget(unit))
                return false;

            if (MpPatchGuard.IsLowDetail(unit))
                return true;

            float dist = MpPatchGuard.DistanceToObserver(unit);
            if (dist > MpConfig.PresentationFarM)
                return true;

            if (dist <= MpConfig.PresentationFullM)
            {
                if (MpConfig.ComponentFullOffscreenSkip && MpPatchGuard.IsOffScreen(unit.transform.position))
                {
                    return true;
                }

                return false;
            }

            if (dist > MpConfig.PresentationNearM)
                return false;

            int stride = MpConfig.ComponentUpdateStride;
            if (stride <= 1)
                return false;

            int slot = GetUnitSlot(unit);
            return Time.frameCount % stride != slot % stride;
        }

        private static bool ShouldSkipDeepVisualUpdate(Unit unit)
        {
            if (MpDeepFreezeManager.IsFrozen(unit))
            {
                return true;
            }

            if (!MpConfig.DeepFreezeEnabled)
                return false;

            float dist = MpPatchGuard.DistanceToObserver(unit);
            if (dist <= MpConfig.DeepFreezeMinM)
                return false;

            if (!GameManager.GetLocalAircraft(out Aircraft local) || local == null)
                return false;

            if (MpPresentationExemptGuard.ShouldExempt(unit, local))
                return false;

            return true;
        }

        private static bool ShouldSkipFullZoneVisual(Unit unit, Vector3 worldPosition)
        {
            if (MpConfig.VisualFullOffscreenSkip && MpPatchGuard.IsOffScreen(worldPosition))
            {
                return true;
            }

            int fullStride = MpConfig.VisualFullZoneStride;
            if (fullStride <= 1)
                return false;

            int slot = GetUnitSlot(unit);
            if (Time.frameCount % fullStride == slot % fullStride)
                return false;

            return true;
        }

        private static int GetUnitSlot(Unit unit)
        {
            int id = unit.persistentID.GetHashCode();
            if (id < 0)
                id = -id;
            return id % 997;
        }

        internal static bool ShouldAlwaysRunMissilePresentation(Missile? missile)
        {
            if (missile == null || !MpSessionState.Active)
                return false;

            if (!MpPatchGuard.IsPresentationUnit(missile))
                return false;

            Unit? owner = missile.owner;
            if (owner != null && GameManager.IsLocalAircraft(owner))
                return true;

            if (!GameManager.GetLocalAircraft(out Aircraft local) || local == null)
                return false;

            if (MpPresentationExemptGuard.ShouldExempt(missile, local))
                return true;

            return MpPatchGuard.DistanceToObserver(missile) <= MpConfig.PresentationFarM;
        }

        internal static bool ShouldAlwaysRunVisualUpdate(Unit? unit)
        {
            if (unit is Missile missile)
                return ShouldAlwaysRunMissilePresentation(missile);

            if (unit == null || !MpSessionState.Active)
                return false;

            if (!MpPatchGuard.IsPresentationUnit(unit))
                return false;

            if (MpPatchGuard.IsLocalSelectedTarget(unit) || MpPatchGuard.IsPresentationExempt(unit))
                return true;

            return MpPatchGuard.DistanceToObserver(unit) <= MpConfig.PresentationFarM;
        }
    }
}
