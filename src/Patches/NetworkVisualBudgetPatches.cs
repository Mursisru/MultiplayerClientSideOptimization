using NuclearOption.NetworkTransforms;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class NetworkVisualBudgetPatches
    {
        public static bool AircraftVisualUpdatePrefixSkip(
            AircraftNetworkTransform __instance,
            ref VisualUpdateTime visualTime)
        {
            Aircraft? aircraft = __instance?.Aircraft;
            MpBufferUnitMap.Register(__instance?.SnapshotBuffer, aircraft);
            if (!ShouldSkip(aircraft))
                return true;

            MpStats.VisualUpdateSkipped++;
            return false;
        }

        public static bool MissileVisualUpdatePrefixSkip(
            MissileNetworkTransform __instance,
            ref VisualUpdateTime visualTime)
        {
            Missile? missile = __instance?.Missile;
            MpBufferUnitMap.Register(__instance?.SnapshotBuffer, missile);
            if (!ShouldSkip(missile))
                return true;

            MpStats.VisualUpdateSkipped++;
            return false;
        }

        public static bool UnitVisualUpdatePrefixSkip(
            UnitNetworkTransform __instance,
            ref VisualUpdateTime visualTime)
        {
            Unit? unit = __instance?.Unit;
            MpBufferUnitMap.Register(__instance?.SnapshotBuffer, unit);
            if (!ShouldSkip(unit))
                return true;

            MpStats.VisualUpdateSkipped++;
            return false;
        }

        public static bool GroundVehicleVisualUpdatePrefixSkip(
            GroundVehicleNetworkTransform __instance,
            ref VisualUpdateTime visualTime)
        {
            GroundVehicle? vehicle = __instance?.GroundVehicle;
            MpBufferUnitMap.Register(__instance?.SnapshotBuffer, vehicle);
            if (!ShouldSkip(vehicle))
                return true;

            MpStats.VisualUpdateSkipped++;
            return false;
        }

        public static bool ShipVisualUpdatePrefixSkip(
            ShipNetworkTransform __instance,
            ref VisualUpdateTime visualTime)
        {
            Ship? ship = __instance?.Ship;
            MpBufferUnitMap.Register(__instance?.SnapshotBuffer, ship);
            if (!ShouldSkip(ship))
                return true;

            MpStats.VisualUpdateSkipped++;
            return false;
        }

        private static bool ShouldSkip(Unit? unit)
        {
            if (unit == null)
                return false;

            return MpVisualBudget.ShouldSkipVisualUpdate(unit, unit.transform.position);
        }
    }
}
