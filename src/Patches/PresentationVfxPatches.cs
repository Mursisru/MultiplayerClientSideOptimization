using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class PresentationVfxPatches
    {
        public static bool VaporEffectFixedUpdatePrefixSkip(VaporEffect __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            Aircraft? aircraft = __instance.GetComponentInParent<Aircraft>();
            if (ShouldSkipDeepCull(aircraft))
                return false;

            if (!MpPatchGuard.ShouldSkipPresentationVfx(aircraft))
                return true;

            MpStats.VaporSkipped++;
            return false;
        }

        public static bool DownwashEffectUpdatePrefixSkip(DownwashEffect __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            Aircraft? aircraft = __instance.GetComponentInParent<Aircraft>();
            if (ShouldSkipDeepCull(aircraft))
                return false;

            if (!MpPatchGuard.ShouldSkipPresentationVfx(aircraft))
                return true;

            MpStats.DownwashSkipped++;
            return false;
        }

        public static bool DownwashEffectSlowUpdatePrefixSkip(DownwashEffect __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            Aircraft? aircraft = __instance.GetComponentInParent<Aircraft>();
            if (ShouldSkipDeepCull(aircraft))
                return false;

            if (aircraft == null || !MpPatchGuard.IsPresentationUnit(aircraft))
                return true;

            if (!MpPatchGuard.IsBeyondPresentationFar(aircraft))
                return true;

            MpStats.DownwashSkipped++;
            return false;
        }

        public static bool TurbineEngineUpdatePrefixSkip(TurbineEngine __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            Aircraft? aircraft = __instance.aircraft;
            if (aircraft == null)
                return true;

            if (ShouldSkipDeepCull(aircraft))
                return false;

            if (!MpPatchGuard.IsPresentationUnit(aircraft))
                return true;

            if (!MpPatchGuard.IsLowDetail(aircraft) && !MpPatchGuard.IsBeyondPresentationFar(aircraft))
                return true;

            MpStats.TurbineSkipped++;
            return false;
        }

        public static bool ShipPropulsionFixedUpdatePrefixSkip(ShipPropulsion __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            Ship? ship = __instance.GetComponentInParent<Ship>();
            if (ship == null || ship.LocalSim)
                return true;

            if (ShouldSkipDeepCull(ship))
                return false;

            MpStats.ShipPropulsionSkipped++;
            return false;
        }

        public static bool GroundVehicleUpdatePrefixSkip(GroundVehicle __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            if (!MpPatchGuard.IsPresentationUnit(__instance))
                return true;

            if (!MpVisualBudget.ShouldSkipPresentationComponent(__instance))
                return true;

            MpStats.GroundVehicleUpdateSkipped++;
            return false;
        }

        public static bool JetNozzleSlowUpdatePrefixSkip(JetNozzle __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            Aircraft? aircraft = __instance.GetComponentInParent<Aircraft>();

            if (ShouldSkipDeepCull(aircraft))
                return false;

            if (!MpPatchGuard.ShouldSkipPresentationVfx(aircraft))
                return true;

            MpStats.JetNozzleSlowUpdateSkipped++;
            return false;
        }

        public static bool FlareEjectorUpdatePrefixSkip(FlareEjector __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            return !TrySkipCountermeasureUpdate(__instance.aircraft, ref MpStats.FlareEjectorUpdateSkipped);
        }

        public static bool ChaffEjectorUpdatePrefixSkip(ChaffEjector __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            return !TrySkipCountermeasureUpdate(__instance.aircraft, ref MpStats.ChaffEjectorUpdateSkipped);
        }

        private static bool TrySkipCountermeasureUpdate(Aircraft? aircraft, ref long counter)
        {
            if (aircraft == null || !MpPatchGuard.IsPresentationUnit(aircraft))
                return false;

            if (!MpVisualBudget.ShouldSkipPresentationComponent(aircraft))
                return false;

            counter++;
            return true;
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
