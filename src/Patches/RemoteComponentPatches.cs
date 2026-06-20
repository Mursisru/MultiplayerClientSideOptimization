using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class RemoteComponentPatches
    {
        public static bool TurbofanFixedUpdatePrefixSkip(Turbofan __instance) =>
            !TrySkipRemoteAircraftComponent(__instance);

        public static bool TurbojetFixedUpdatePrefixSkip(Turbojet __instance) =>
            !TrySkipRemoteAircraftComponent(__instance);

        public static bool PropFanFixedUpdatePrefixSkip(PropFan __instance) =>
            !TrySkipRemoteAircraftComponent(__instance);

        public static bool ConstantSpeedPropFixedUpdatePrefixSkip(ConstantSpeedProp __instance) =>
            !TrySkipRemoteAircraftComponent(__instance);

        public static bool LandingGearFixedUpdatePrefixSkip(LandingGear __instance) =>
            !TrySkipRemoteAircraftComponent(__instance);

        public static bool HighLiftDeviceFixedUpdatePrefixSkip(HighLiftDevice __instance) =>
            !TrySkipRemoteAircraftComponent(__instance);

        public static bool AirbrakeFixedUpdatePrefixSkip(Airbrake __instance) =>
            !TrySkipRemoteAircraftComponent(__instance);

        public static bool ControlSurfacePhysicsFixedUpdatePrefixSkip(ControlSurfacePhysics __instance) =>
            !TrySkipRemoteAircraftComponent(__instance);

        public static bool FuelTankFixedUpdatePrefixSkip(FuelTank __instance) =>
            !TrySkipRemoteAircraft(__instance?.GetComponentInParent<Aircraft>());

        public static bool RotorShaftFixedUpdatePrefixSkip(RotorShaft __instance) =>
            !TrySkipRemoteAircraft(__instance?.aircraft);

        public static bool RotorShaftUpdatePrefixSkip(RotorShaft __instance) =>
            !TrySkipRemoteAircraft(__instance?.aircraft);

        public static bool PilotUpdatePrefixSkip(Pilot __instance) =>
            !TrySkipRemoteAircraft(__instance?.aircraft);

        public static bool TurretFixedUpdatePrefixSkip(Turret __instance) =>
            !TrySkipPresentationShell(__instance?.GetComponentInParent<Unit>());

        private static bool TrySkipRemoteAircraftComponent(MonoBehaviour? component)
        {
            if (!MpSessionState.Active || component == null)
                return false;

            Aircraft? aircraft = component.GetComponentInParent<Aircraft>();
            return TrySkipRemoteAircraft(aircraft);
        }

        private static bool TrySkipRemoteAircraft(Aircraft? aircraft)
        {
            if (!MpPatchGuard.ShouldSkipRemoteComponent(aircraft))
                return false;

            MpStats.RemoteComponentSkipped++;
            return true;
        }

        private static bool TrySkipPresentationShell(Unit? unit)
        {
            if (!MpPatchGuard.ShouldSkipPresentationShell(unit))
                return false;

            MpStats.RemoteComponentSkipped++;
            return true;
        }
    }
}
