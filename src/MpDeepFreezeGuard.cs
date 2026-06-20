using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpDeepFreezeGuard
    {
        internal static bool IsEligibleCandidate(Unit? unit)
        {
            if (unit == null || unit.disabled || !MpSessionState.Active)
                return false;

            if (!MpPatchGuard.IsPresentationUnit(unit))
                return false;

            return unit.rb != null;
        }

        internal static bool ShouldExemptFromFreeze(Unit? unit, Aircraft? local) =>
            MpPresentationExemptGuard.ShouldExempt(unit, local);

        internal static bool ShouldApplyFreeze(Unit? unit, Aircraft? local)
        {
            if (!IsEligibleCandidate(unit) || local == null)
                return false;

            return MpPresentationExemptGuard.ShouldApplyDeepCull(unit, local);
        }

        internal static bool IsIncomingMissile(Missile? missile, Aircraft local) =>
            MpPresentationExemptGuard.IsIncomingMissile(missile, local);
    }
}
