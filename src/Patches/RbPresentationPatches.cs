using NuclearOption.NetworkTransforms;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class RbPresentationPatches
    {
        public static bool AircraftApplySnapshotPrefixSkip(
            AircraftNetworkTransform __instance,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (!ShouldSkipRbOps(__instance?.Aircraft))
                return true;

            MpRbPresentationHelper.ApplyAircraftTransformOnly(__instance, snapshot);
            MpStats.RbApplySkipped++;
            return false;
        }

        public static bool MissileApplySnapshotPrefixSkip(
            MissileNetworkTransform __instance,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (!ShouldSkipRbOps(__instance?.Missile))
                return true;

            MpRbPresentationHelper.ApplyMissileTransformOnly(__instance, snapshot);
            MpStats.RbApplySkipped++;
            return false;
        }

        public static bool GroundVehicleApplySnapshotPrefixSkip(
            GroundVehicleNetworkTransform __instance,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (!ShouldSkipRbOps(__instance?.GroundVehicle))
                return true;

            MpRbPresentationHelper.ApplyGroundVehicleTransformOnly(__instance, snapshot);
            MpStats.RbApplySkipped++;
            return false;
        }

        public static bool UnitApplySnapshotPrefixSkip(
            UnitNetworkTransform __instance,
            NetworkTransformBase.ViewSnapshot snapshot,
            bool snap)
        {
            if (!ShouldSkipRbOps(__instance?.Unit))
                return true;

            MpRbPresentationHelper.ApplyUnitTransformOnly(__instance, snapshot);
            MpStats.RbApplySkipped++;
            return false;
        }

        private static bool ShouldSkipRbOps(Unit? unit)
        {
            if (!MpSessionState.Active || unit == null)
                return false;

            if (!MpPatchGuard.IsPresentationUnit(unit))
                return false;

            return MpPatchGuard.ShouldSkipRbPresentation(unit);
        }
    }
}
