using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class CombatVfxPatches
    {
        public static bool EmitParticlesPrefixSkip(
            ParticleEffectManager __instance,
            string name,
            int number,
            GlobalPosition origin,
            Vector3 startVelocity,
            float positionVariation,
            float lifetime,
            float lifetimeVariation,
            float startSize,
            float sizeVariation,
            float velocityVariation,
            float opacity,
            float opacityVariation)
        {
            if (!MpSessionState.Active)
                return true;

            Vector3 world = origin.AsVector3();
            if (!MpPatchGuard.TryGetObserverPosition(out Vector3 observer))
                return true;

            float dist = Vector3.Distance(observer, world);
            if (dist <= MpConfig.FxMaxDistanceM)
                return true;

            if (MpConfig.DeepFreezeEnabled && dist > MpConfig.DeepFreezeMinM)
            {
                MpStats.EmitParticlesSkipped++;
                return false;
            }

            MpStats.EmitParticlesSkipped++;
            return false;
        }

        public static bool DamageParticlesUpdatePrefixSkip(DamageParticles __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            UnitPart? part = __instance.GetComponentInParent<UnitPart>();
            Unit? parent = part?.parentUnit;
            if (parent == null)
                return true;

            if (GameManager.IsLocalAircraft(parent))
                return true;

            if (ShouldSkipDeepCull(parent))
                return false;

            if (!MpPatchGuard.IsBeyondFxRange(parent))
                return true;

            MpStats.DamageParticlesSkipped++;
            return false;
        }

        private static bool ShouldSkipDeepCull(Unit? unit)
        {
            if (unit == null || !MpConfig.DeepFreezeEnabled)
                return false;

            if (!GameManager.GetLocalAircraft(out Aircraft local) || local == null)
                return false;

            if (!MpPresentationExemptGuard.ShouldApplyDeepCull(unit, local))
                return false;

            MpStats.DeepCullVfxSkipped++;
            return true;
        }
    }
}
