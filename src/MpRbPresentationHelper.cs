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

            SyncRigidbodyVelocity(nt.Aircraft?.rb, snapshot.Velocity);

            Vector3 position = snapshot.Position;
            Quaternion rotation = snapshot.Rotation;
            if (nt.SendBatcher != null && nt.SendBatcher.IsCloseToCamera(position))
                nt.transform.SetPositionAndRotation(position, rotation);
        }

        internal static void ApplyMissileTransformOnly(
            MissileNetworkTransform nt,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (nt == null || nt.Missile == null)
                return;

            SyncRigidbodyVelocity(nt.Missile.rb, snapshot.Velocity);

            Vector3 position = snapshot.Position;
            Vector3 velocity = snapshot.Velocity;
            Quaternion rotation = global::FastMath.LookRotation(velocity);
            if (nt.SendBatcher != null && nt.SendBatcher.IsCloseToCamera(position))
                nt.transform.SetPositionAndRotation(position, rotation);
        }

        internal static void ApplyGroundVehicleTransformOnly(
            GroundVehicleNetworkTransform nt,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (nt == null)
                return;

            SyncRigidbodyVelocity(nt.GroundVehicle?.rb, snapshot.Velocity);

            Vector3 position = snapshot.Position;
            Quaternion rotation = snapshot.Rotation;
            if (nt.SendBatcher != null && nt.SendBatcher.IsCloseToCamera(position))
                nt.transform.SetPositionAndRotation(position, rotation);
        }

        internal static void ApplyUnitTransformOnly(
            UnitNetworkTransform nt,
            NetworkTransformBase.ViewSnapshot snapshot)
        {
            if (nt == null)
                return;

            SyncRigidbodyVelocity(nt.Unit?.rb, snapshot.Velocity);

            Vector3 position = snapshot.Position;
            Quaternion rotation = snapshot.Rotation;
            nt.transform.SetPositionAndRotation(position, rotation);
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
