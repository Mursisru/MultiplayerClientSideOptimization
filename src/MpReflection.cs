using System.Reflection;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpReflection
    {
        private static readonly FieldInfo? MissileFlightSoundField =
            typeof(Missile).GetField("flightSound", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo? AircraftSpawnedInPositionField =
            typeof(Aircraft).GetField("spawnedInPosition", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo? LaserFireCommandedField =
            typeof(Laser).GetField("fireCommanded", BindingFlags.Instance | BindingFlags.NonPublic);

        internal static AudioSource? GetMissileFlightSound(Missile missile)
        {
            if (missile == null || MissileFlightSoundField == null)
                return null;
            return MissileFlightSoundField.GetValue(missile) as AudioSource;
        }

        internal static void EnsureMissileRigidbodyAwake(Missile missile)
        {
            if (missile == null)
                return;

            Rigidbody? rb = missile.rb;
            if (rb == null)
                return;

            if (rb.isKinematic)
                rb.isKinematic = false;

            if (!rb.detectCollisions)
                rb.detectCollisions = true;
        }

        internal static bool IsAircraftSpawnedInPosition(Aircraft aircraft)
        {
            if (aircraft == null || AircraftSpawnedInPositionField == null)
                return false;
            object? value = AircraftSpawnedInPositionField.GetValue(aircraft);
            return value is bool spawned && spawned;
        }

        internal static bool IsLaserFireCommanded(Laser laser)
        {
            if (laser == null || LaserFireCommandedField == null)
                return false;
            object? value = LaserFireCommandedField.GetValue(laser);
            return value is bool commanded && commanded;
        }
    }
}
