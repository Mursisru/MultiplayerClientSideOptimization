using HarmonyLib;
using NOLoader.ModConfig;
using NOLoader.MultiplayerClientSideOptimization;
using NOLoader.MultiplayerClientSideOptimization.Patches;
using NuclearOption.NetworkTransforms;

namespace MultiplayerClientSideOptimization.BepInEx
{
    internal static class PatchBootstrap
    {
        internal static void Apply(Harmony harmony)
        {
            harmony.PatchAll(typeof(PatchBootstrap).Assembly);
        }
    }

    [HarmonyPatch(typeof(VaporEffect), "FixedUpdate")]
    internal static class VaporEffect_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(VaporEffect __instance) =>
            PresentationVfxPatches.VaporEffectFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(DownwashEffect), "Update")]
    internal static class DownwashEffect_Update_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(DownwashEffect __instance) =>
            PresentationVfxPatches.DownwashEffectUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(DownwashEffect), "SlowUpdate")]
    internal static class DownwashEffect_SlowUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(DownwashEffect __instance) =>
            PresentationVfxPatches.DownwashEffectSlowUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(TurbineEngine), "Update")]
    internal static class TurbineEngine_Update_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(TurbineEngine __instance) =>
            PresentationVfxPatches.TurbineEngineUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(ShipPropulsion), "FixedUpdate")]
    internal static class ShipPropulsion_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(ShipPropulsion __instance) =>
            PresentationVfxPatches.ShipPropulsionFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Aircraft), "FixedUpdate")]
    internal static class Aircraft_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Aircraft __instance) =>
            AircraftPartialPatches.FixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Aircraft), "ShakeAircraft")]
    internal static class Aircraft_ShakeAircraft_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Aircraft __instance, float lowFreqShake, float highFreqShake) =>
            AircraftPartialPatches.ShakeAircraftPrefixSkip(__instance, lowFreqShake, highFreqShake);
    }

    [HarmonyPatch(typeof(Missile), "MotorThrust")]
    internal static class Missile_MotorThrust_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Missile __instance) =>
            MissileDistancePatches.MotorThrustPrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Missile), "FixedUpdate")]
    internal static class Missile_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Missile __instance) =>
            MissileDistancePatches.FixedUpdatePrefixSkip(__instance);

        [HarmonyPostfix]
        internal static void Postfix(Missile __instance) =>
            MissileDistancePatches.FixedUpdatePostfix(__instance);
    }

    [HarmonyPatch(typeof(AircraftNetworkTransform), "ApplySnapshot")]
    internal static class AircraftNetworkTransform_ApplySnapshot_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            AircraftNetworkTransform __instance,
            NetworkTransformBase.ViewSnapshot snapshot) =>
            RbPresentationPatches.AircraftApplySnapshotPrefixSkip(__instance, snapshot);
    }

    [HarmonyPatch(typeof(MissileNetworkTransform), "ApplySnapshot")]
    internal static class MissileNetworkTransform_ApplySnapshot_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            MissileNetworkTransform __instance,
            NetworkTransformBase.ViewSnapshot snapshot) =>
            RbPresentationPatches.MissileApplySnapshotPrefixSkip(__instance, snapshot);
    }

    [HarmonyPatch(typeof(GroundVehicleNetworkTransform), "ApplySnapshot")]
    internal static class GroundVehicleNetworkTransform_ApplySnapshot_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            GroundVehicleNetworkTransform __instance,
            NetworkTransformBase.ViewSnapshot snapshot) =>
            RbPresentationPatches.GroundVehicleApplySnapshotPrefixSkip(__instance, snapshot);
    }

    [HarmonyPatch(typeof(UnitNetworkTransform), "ApplySnapshot")]
    internal static class UnitNetworkTransform_ApplySnapshot_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            UnitNetworkTransform __instance,
            NetworkTransformBase.ViewSnapshot snapshot,
            bool snap) =>
            RbPresentationPatches.UnitApplySnapshotPrefixSkip(__instance, snapshot, snap);
    }

    [HarmonyPatch(typeof(AircraftNetworkTransform), "VisualUpdate")]
    internal static class AircraftNetworkTransform_VisualUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            AircraftNetworkTransform __instance,
            ref VisualUpdateTime visualTime) =>
            NetworkVisualBudgetPatches.AircraftVisualUpdatePrefixSkip(__instance, ref visualTime);
    }

    [HarmonyPatch(typeof(MissileNetworkTransform), "VisualUpdate")]
    internal static class MissileNetworkTransform_VisualUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            MissileNetworkTransform __instance,
            ref VisualUpdateTime visualTime) =>
            NetworkVisualBudgetPatches.MissileVisualUpdatePrefixSkip(__instance, ref visualTime);
    }

    [HarmonyPatch(typeof(UnitNetworkTransform), "VisualUpdate")]
    internal static class UnitNetworkTransform_VisualUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            UnitNetworkTransform __instance,
            ref VisualUpdateTime visualTime) =>
            NetworkVisualBudgetPatches.UnitVisualUpdatePrefixSkip(__instance, ref visualTime);
    }

    [HarmonyPatch(typeof(GroundVehicleNetworkTransform), "VisualUpdate")]
    internal static class GroundVehicleNetworkTransform_VisualUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            GroundVehicleNetworkTransform __instance,
            ref VisualUpdateTime visualTime) =>
            NetworkVisualBudgetPatches.GroundVehicleVisualUpdatePrefixSkip(__instance, ref visualTime);
    }

    [HarmonyPatch(typeof(ShipNetworkTransform), "VisualUpdate")]
    internal static class ShipNetworkTransform_VisualUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            ShipNetworkTransform __instance,
            ref VisualUpdateTime visualTime) =>
            NetworkVisualBudgetPatches.ShipVisualUpdatePrefixSkip(__instance, ref visualTime);
    }

    [HarmonyPatch(typeof(Turbofan), "FixedUpdate")]
    internal static class Turbofan_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Turbofan __instance) =>
            RemoteComponentPatches.TurbofanFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Turbojet), "FixedUpdate")]
    internal static class Turbojet_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Turbojet __instance) =>
            RemoteComponentPatches.TurbojetFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(PropFan), "FixedUpdate")]
    internal static class PropFan_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(PropFan __instance) =>
            RemoteComponentPatches.PropFanFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(ConstantSpeedProp), "FixedUpdate")]
    internal static class ConstantSpeedProp_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(ConstantSpeedProp __instance) =>
            RemoteComponentPatches.ConstantSpeedPropFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(LandingGear), "FixedUpdate")]
    internal static class LandingGear_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(LandingGear __instance) =>
            RemoteComponentPatches.LandingGearFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(HighLiftDevice), "FixedUpdate")]
    internal static class HighLiftDevice_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(HighLiftDevice __instance) =>
            RemoteComponentPatches.HighLiftDeviceFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Airbrake), "FixedUpdate")]
    internal static class Airbrake_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Airbrake __instance) =>
            RemoteComponentPatches.AirbrakeFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(ControlSurfacePhysics), "FixedUpdate")]
    internal static class ControlSurfacePhysics_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(ControlSurfacePhysics __instance) =>
            RemoteComponentPatches.ControlSurfacePhysicsFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(FuelTank), "FixedUpdate")]
    internal static class FuelTank_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(FuelTank __instance) =>
            RemoteComponentPatches.FuelTankFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(RotorShaft), "FixedUpdate")]
    internal static class RotorShaft_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(RotorShaft __instance) =>
            RemoteComponentPatches.RotorShaftFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(RotorShaft), "Update")]
    internal static class RotorShaft_Update_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(RotorShaft __instance) =>
            RemoteComponentPatches.RotorShaftUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Pilot), "Update")]
    internal static class Pilot_Update_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Pilot __instance) =>
            RemoteComponentPatches.PilotUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Turret), "FixedUpdate")]
    internal static class Turret_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Turret __instance) =>
            RemoteComponentPatches.TurretFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(ParticleEffectManager), "EmitParticles")]
    internal static class ParticleEffectManager_EmitParticles_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(
            ParticleEffectManager __instance,
            string name,
            int number,
            GlobalPosition origin,
            UnityEngine.Vector3 startVelocity,
            float positionVariation,
            float lifetime,
            float lifetimeVariation,
            float startSize,
            float sizeVariation,
            float velocityVariation,
            float opacity,
            float opacityVariation) =>
            CombatVfxPatches.EmitParticlesPrefixSkip(
                __instance, name, number, origin, startVelocity, positionVariation, lifetime,
                lifetimeVariation, startSize, sizeVariation, velocityVariation, opacity, opacityVariation);
    }

    [HarmonyPatch(typeof(DamageParticles), "Update")]
    internal static class DamageParticles_Update_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(DamageParticles __instance) =>
            CombatVfxPatches.DamageParticlesUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(NetworkTransformBase.SnapshotBufferLocalSnapshot), "RemoveOld")]
    internal static class SnapshotBuffer_RemoveOld_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(object __instance, double timestamp) =>
            SnapshotBufferPatches.RemoveOldPrefixSkip(__instance, timestamp);
    }

    [HarmonyPatch(typeof(Gun), "FixedUpdate")]
    internal static class Gun_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Gun __instance) =>
            WeaponShellPatches.GunFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Gun), "Update")]
    internal static class Gun_Update_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Gun __instance) =>
            WeaponShellPatches.GunUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Laser), "FixedUpdate")]
    internal static class Laser_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Laser __instance) =>
            WeaponShellPatches.LaserFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Laser), "LateUpdate")]
    internal static class Laser_LateUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Laser __instance) =>
            WeaponShellPatches.LaserLateUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(JammingPod), "FixedUpdate")]
    internal static class JammingPod_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(JammingPod __instance) =>
            WeaponShellPatches.JammingPodFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(JammingPod), "LateUpdate")]
    internal static class JammingPod_LateUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(JammingPod __instance) =>
            WeaponShellPatches.JammingPodLateUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Radar), "Update")]
    internal static class Radar_Update_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Radar __instance) =>
            WeaponShellPatches.RadarUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Repulsorlift), "FixedUpdate")]
    internal static class Repulsorlift_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Repulsorlift __instance) =>
            WeaponShellPatches.RepulsorliftFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(AirCushion), "FixedUpdate")]
    internal static class AirCushion_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(AirCushion __instance) =>
            WeaponShellPatches.AirCushionFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Transmission), "FixedUpdate")]
    internal static class Transmission_FixedUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Transmission __instance) =>
            WeaponShellPatches.TransmissionFixedUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(Aircraft), "CheckSpawnedInPosition")]
    internal static class Aircraft_CheckSpawnedInPosition_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Aircraft __instance) =>
            WeaponShellPatches.CheckSpawnedInPositionPrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(SendTransformBatcher), "VisualUpdate")]
    internal static class SendTransformBatcher_VisualUpdate_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(SendTransformBatcher __instance) =>
            SendTransformBatcherRedirect.VisualUpdatePrefixSkip(__instance);
    }

    [HarmonyPatch(typeof(ControlSurface), "UpdateJobFields")]
    internal static class ControlSurface_UpdateJobFields_Patch
    {
        [HarmonyPrefix]
        internal static bool Prefix(ControlSurface __instance) =>
            ControlSurfaceJobPatches.UpdateJobFieldsPrefixSkip(__instance);
    }
}
