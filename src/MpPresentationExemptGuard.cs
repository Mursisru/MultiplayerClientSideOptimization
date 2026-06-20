using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpPresentationExemptGuard
    {
        internal static bool ShouldExempt(Unit? unit, Aircraft? local)
        {
            if (unit == null || local == null)
                return true;

            if (MpPatchGuard.DistanceToObserver(unit) <= MpConfig.DeepFreezeMinM)
                return true;

            WeaponManager? wm = local.weaponManager;
            if (wm != null && wm.CheckIsTarget(unit))
                return true;

            if (unit.CheckIsTarget(local))
                return true;

            if (unit.NetworkHQ != null && unit.NetworkHQ.IsTargetBeingTracked(local))
                return true;

            if (unit is Missile missile)
            {
                Unit? owner = missile.owner;
                if (owner != null && GameManager.IsLocalAircraft(owner))
                    return true;

                if (IsIncomingMissile(missile, local))
                    return true;
            }

            return false;
        }

        internal static bool ShouldApplyDeepCull(Unit? unit, Aircraft? local)
        {
            if (unit == null || local == null || !MpSessionState.Active)
                return false;

            if (!MpPatchGuard.IsPresentationUnit(unit))
                return false;

            if (ShouldExempt(unit, local))
                return false;

            return MpPatchGuard.DistanceToObserver(unit) > MpConfig.DeepFreezeMinM;
        }

        internal static bool IsIncomingMissile(Missile? missile, Aircraft local)
        {
            if (missile == null || local == null)
                return false;

            if (missile.targetID == local.persistentID)
                return true;

            if (missile.targetID.IsValid
                && UnitRegistry.TryGetUnit(new PersistentID?(missile.targetID), out Unit? target)
                && target == local)
                return true;

            Rigidbody? rb = missile.rb;
            if (rb == null)
                return false;

            float dist = Vector3.Distance(missile.transform.position, local.transform.position);
            if (dist > MpConfig.IncomingMissileMaxM)
                return false;

            Vector3 toLocal = local.transform.position - missile.transform.position;
            if (toLocal.sqrMagnitude < 1f)
                return true;

            Vector3 velocity = rb.velocity;
            if (velocity.sqrMagnitude < 1f)
                return false;

            float dot = Vector3.Dot(velocity.normalized, toLocal.normalized);
            return dot >= MpConfig.IncomingMissileDotMin;
        }
    }
}
