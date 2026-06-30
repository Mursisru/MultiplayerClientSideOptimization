using NuclearOption.NetworkTransforms;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpRbPresentationHelper
    {
        internal static void ApplyAircraftTransformOnly(
            AircraftNetworkTransform nt,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (nt == null)
                return;

            ApplyPresentationSnapshot(
                nt.transform,
                nt.Aircraft?.rb,
                nt.SendBatcher,
                snapshot.Position,
                snapshot.Rotation,
                snapshot.Velocity);
        }

        internal static void ApplyMissileTransformOnly(
            MissileNetworkTransform nt,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (nt == null || nt.Missile == null)
                return;

            Vector3 velocity = snapshot.Velocity;
            ApplyPresentationSnapshot(
                nt.transform,
                nt.Missile.rb,
                nt.SendBatcher,
                snapshot.Position,
                global::FastMath.LookRotation(velocity),
                velocity);
        }

        internal static void ApplyGroundVehicleTransformOnly(
            GroundVehicleNetworkTransform nt,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (nt == null)
                return;

            ApplyPresentationSnapshot(
                nt.transform,
                nt.GroundVehicle?.rb,
                nt.SendBatcher,
                snapshot.Position,
                snapshot.Rotation,
                snapshot.Velocity);
        }

        internal static void ApplyUnitTransformOnly(
            UnitNetworkTransform nt,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (nt == null)
                return;

            ApplyPresentationSnapshot(
                nt.transform,
                nt.Unit?.rb,
                nt.SendBatcher,
                snapshot.Position,
                snapshot.Rotation,
                snapshot.Velocity);
        }

        private static void ApplyPresentationSnapshot(
            Transform transform,
            Rigidbody? rb,
            SendTransformBatcher? batcher,
            Vector3 position,
            Quaternion rotation,
            Vector3 velocity)
        {
            SyncRigidbodyVelocity(rb, velocity);

            if (batcher != null && batcher.IsCloseToCamera(position))
                transform.SetPositionAndRotation(position, rotation);
            else if (rb != null)
                rb.Move(position, rotation);
            else
                transform.SetPositionAndRotation(position, rotation);
        }

        private static void SyncRigidbodyVelocity(Rigidbody? rb, Vector3 velocity)
        {
            if (rb == null)
                return;

            rb.velocity = velocity;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
