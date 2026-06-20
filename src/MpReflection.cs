using System.Reflection;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpReflection
    {
        private static readonly FieldInfo? MissileFlightSoundField =
            typeof(Missile).GetField("flightSound", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly FieldInfo? MissileTimeSinceSpawnField =
            typeof(Missile).GetField("<timeSinceSpawn>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);

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

        internal static void IncrementMissileTimeSinceSpawn(Missile missile, float deltaTime)
        {
            if (missile == null || MissileTimeSinceSpawnField == null || deltaTime <= 0f)
                return;

            object? current = MissileTimeSinceSpawnField.GetValue(missile);
            float value = current is float f ? f : 0f;
            MissileTimeSinceSpawnField.SetValue(missile, value + deltaTime);
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
