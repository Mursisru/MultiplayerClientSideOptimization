using NuclearOption.NetworkTransforms;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpNetworkTransformHelper
    {
        internal static bool TryGetUnit(NetworkTransformBase? nt, out Unit? unit)
        {
            unit = null;
            if (nt == null)
                return false;

            switch (nt)
            {
                case AircraftNetworkTransform aircraftNt:
                    unit = aircraftNt.Aircraft;
                    return unit != null;
                case MissileNetworkTransform missileNt:
                    unit = missileNt.Missile;
                    return unit != null;
                case GroundVehicleNetworkTransform gvNt:
                    unit = gvNt.GroundVehicle;
                    return unit != null;
                case ShipNetworkTransform shipNt:
                    unit = shipNt.Ship;
                    return unit != null;
                case UnitNetworkTransform unitNt:
                    unit = unitNt.Unit;
                    return unit != null;
                default:
                    return false;
            }
        }

        internal static NetworkTransformBase? TryGetNetworkTransform(Unit? unit)
        {
            if (unit == null)
                return null;

            NetworkTransformBase? nt = unit.GetComponentInChildren<AircraftNetworkTransform>(true);
            if (nt != null)
                return nt;

            nt = unit.GetComponentInChildren<MissileNetworkTransform>(true);
            if (nt != null)
                return nt;

            nt = unit.GetComponentInChildren<GroundVehicleNetworkTransform>(true);
            if (nt != null)
                return nt;

            nt = unit.GetComponentInChildren<ShipNetworkTransform>(true);
            if (nt != null)
                return nt;

            return unit.GetComponentInChildren<UnitNetworkTransform>(true);
        }
    }
}
