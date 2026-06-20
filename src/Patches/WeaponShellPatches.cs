using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class WeaponShellPatches
    {
        public static bool GunUpdatePrefixSkip(Gun __instance) =>
            !TrySkipWeaponShell(__instance?.attachedUnit);

        public static bool GunFixedUpdatePrefixSkip(Gun __instance) =>
            !TrySkipWeaponShell(__instance?.attachedUnit);

        public static bool LaserLateUpdatePrefixSkip(Laser __instance)
        {
            Unit? unit = __instance?.attachedUnit;
            if (unit == null || !MpPatchGuard.IsPresentationUnit(unit))
                return true;

            if (MpReflection.IsLaserFireCommanded(__instance))
                return true;

            return !TrySkipWeaponShell(unit);
        }

        public static bool LaserFixedUpdatePrefixSkip(Laser __instance)
        {
            Unit? unit = __instance?.attachedUnit;
            if (unit == null || !MpPatchGuard.IsPresentationUnit(unit))
                return true;

            if (MpReflection.IsLaserFireCommanded(__instance))
                return true;

            return !TrySkipWeaponShell(unit);
        }

        public static bool JammingPodLateUpdatePrefixSkip(JammingPod __instance) =>
            !TrySkipWeaponShell(__instance?.attachedUnit);

        public static bool JammingPodFixedUpdatePrefixSkip(JammingPod __instance) =>
            !TrySkipWeaponShell(__instance?.attachedUnit);

        public static bool RadarUpdatePrefixSkip(Radar __instance) =>
            !TrySkipWeaponShell(__instance?.GetComponentInParent<Unit>());

        public static bool RepulsorliftFixedUpdatePrefixSkip(Repulsorlift __instance) =>
            !TrySkipAllPresentationAircraft(__instance?.GetComponentInParent<Aircraft>());

        public static bool AirCushionFixedUpdatePrefixSkip(AirCushion __instance) =>
            !TrySkipWeaponShell(__instance?.GetComponentInParent<Unit>());

        public static bool TransmissionFixedUpdatePrefixSkip(Transmission __instance) =>
            !TrySkipWeaponShell(__instance?.GetComponentInParent<Unit>());

        public static bool CheckSpawnedInPositionPrefixSkip(Aircraft __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            if (!MpPatchGuard.IsPresentationUnit(__instance))
                return true;

            if (!MpReflection.IsAircraftSpawnedInPosition(__instance))
                return true;

            MpStats.WeaponShellSkipped++;
            return false;
        }

        private static bool TrySkipWeaponShell(Unit? unit)
        {
            if (!MpVisualBudget.ShouldSkipPresentationComponent(unit))
                return false;

            MpStats.WeaponShellSkipped++;
            return true;
        }

        private static bool TrySkipAllPresentationAircraft(Aircraft? aircraft)
        {
            if (aircraft == null || !MpPatchGuard.IsPresentationUnit(aircraft))
                return false;

            MpStats.WeaponShellSkipped++;
            return true;
        }
    }
}
