using NuclearOption.NetworkTransforms;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpDeepFreezeResync
    {
        internal static void ForceResyncTransform(Unit? unit)
        {
            if (unit == null)
                return;

            NetworkTransformBase? nt = MpNetworkTransformHelper.TryGetNetworkTransform(unit);
            if (nt == null)
                return;

            NetworkTransformBase.SnapshotBufferLocalSnapshot buffer = nt.SnapshotBuffer;
            if (buffer.Count < 1)
                return;

            double snapshotTime = Time.fixedTimeAsDouble - nt.SyncInterval * 2.5;
            NetworkTransformBase.ViewSnapshot view = buffer.GetSnapshotForTime(snapshotTime);

            Rigidbody? rb = unit.rb;
            if (rb != null)
            {
                rb.velocity = view.Velocity;
                rb.angularVelocity = Vector3.zero;
            }

            if (nt.SendBatcher != null && nt.SendBatcher.IsCloseToCamera(view.Position))
                unit.transform.SetPositionAndRotation(view.Position, view.Rotation);
            else if (rb != null)
                rb.Move(view.Position, view.Rotation);
            else
                unit.transform.SetPositionAndRotation(view.Position, view.Rotation);
        }
    }
}
