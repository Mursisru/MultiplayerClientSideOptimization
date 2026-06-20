using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class MissileDistancePatches
    {
        public static bool MotorThrustPrefixSkip(Missile __instance)
        {
            if (!MpSessionState.Active || __instance == null || __instance.LocalSim)
                return true;

            if (!MpPatchGuard.IsBeyondOpticalRange(__instance))
                return true;

            return false;
        }

        public static bool FixedUpdatePrefixSkip(Missile __instance)
        {
            if (!MpSessionState.Active || __instance == null || __instance.LocalSim)
                return true;

            if (!MpPatchGuard.IsBeyondOpticalRange(__instance))
                return true;

            return false;
        }

        public static void FixedUpdatePostfix(Missile __instance)
        {
            if (!MpSessionState.Active || __instance == null || __instance.LocalSim)
                return;

            if (MpPatchGuard.IsBeyondOpticalRange(__instance))
                MpReflection.IncrementMissileTimeSinceSpawn(__instance, Time.fixedDeltaTime);

            MpPatchGuard.TrySleepMissileRigidbody(__instance);

            if (!MpPatchGuard.IsBeyondOpticalRange(__instance))
                return;

            AudioSource? flightSound = MpReflection.GetMissileFlightSound(__instance);
            if (flightSound != null)
            {
                flightSound.volume = 0f;
            }
        }
    }
}
