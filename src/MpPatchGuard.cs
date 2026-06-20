using NuclearOption.Networking;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpPatchGuard
    {
        private static float _cachedOpticalRangeM;
        private static float _opticalCacheTime = -999f;

        internal static bool IsHostOrServerActive()
        {
            NetworkManagerNuclearOption? mgr = NetworkManagerNuclearOption.i;
            if (mgr == null)
                return false;
            return mgr.Server != null && mgr.Server.Active;
        }

        internal static bool IsPresentationUnit(Unit? unit)
        {
            if (unit == null || !MpSessionState.Active)
                return false;
            if (IsHostOrServerActive())
                return false;
            return unit.remoteSim && !GameManager.IsLocalAircraft(unit);
        }

        internal static bool IsLowDetail(Unit unit)
        {
            return unit.displayDetail < 1f;
        }

        internal static bool TryGetObserverPosition(out Vector3 position)
        {
            position = default;
            if (GameManager.GetLocalAircraft(out Aircraft local))
            {
                position = local.transform.position;
                return true;
            }

            Camera? cam = Camera.main;
            if (cam == null)
                return false;

            position = cam.transform.position;
            return true;
        }

        internal static float DistanceToObserver(Unit unit)
        {
            if (unit == null || !TryGetObserverPosition(out Vector3 observer))
                return float.MaxValue;
            return Vector3.Distance(observer, unit.transform.position);
        }

        internal static float GetEffectiveOpticalRangeM()
        {
            float now = Time.unscaledTime;
            if (now - _opticalCacheTime < 1f)
                return _cachedOpticalRangeM;

            _opticalCacheTime = now;
            _cachedOpticalRangeM = ComputeOpticalRangeM();
            return _cachedOpticalRangeM;
        }

        internal static bool IsBeyondOpticalRange(Unit unit)
        {
            return DistanceToObserver(unit) > GetEffectiveOpticalRangeM();
        }

        internal static bool IsBeyondPresentationFar(Unit unit)
        {
            return DistanceToObserver(unit) > MpConfig.PresentationFarM;
        }

        internal static bool IsBeyondFxRange(Unit unit)
        {
            return DistanceToObserver(unit) > MpConfig.FxMaxDistanceM;
        }

        internal static bool IsOffScreen(Vector3 worldPosition)
        {
            Camera? cam = Camera.main;
            if (cam == null)
                return false;

            Vector3 viewport = cam.WorldToViewportPoint(worldPosition);
            if (viewport.z <= 0f)
                return true;

            const float margin = 0.05f;
            return viewport.x < -margin || viewport.x > 1f + margin
                || viewport.y < -margin || viewport.y > 1f + margin;
        }

        internal static bool ShouldSkipRbPresentation(Unit unit)
        {
            if (!MpConfig.RbMoveThrottleEnabled || unit == null)
                return false;

            if (DistanceToObserver(unit) <= MpConfig.RbPhysicsSleepM)
                return false;

            if (!IsOffScreen(unit.transform.position))
                return false;

            int stride = MpConfig.RbMoveStride;
            return stride <= 1 || Time.frameCount % stride != 0;
        }

        internal static bool IsLocalSelectedTarget(Unit? unit)
        {
            if (unit == null || !GameManager.GetLocalAircraft(out Aircraft local) || local == null)
                return false;

            WeaponManager? wm = local.weaponManager;
            return wm != null && wm.CheckIsTarget(unit);
        }

        internal static bool IsPresentationExempt(Unit? unit)
        {
            if (unit == null || !GameManager.GetLocalAircraft(out Aircraft local) || local == null)
                return false;

            return MpPresentationExemptGuard.ShouldExempt(unit, local);
        }

        internal static bool ShouldSkipPresentationVfx(Aircraft? aircraft)
        {
            if (aircraft == null)
                return true;
            if (!IsPresentationUnit(aircraft))
                return false;
            return IsLowDetail(aircraft) || IsBeyondPresentationFar(aircraft);
        }

        internal static bool ShouldSkipRemoteComponent(Aircraft? aircraft)
        {
            if (aircraft == null)
                return false;
            return MpVisualBudget.ShouldSkipPresentationComponent(aircraft);
        }

        internal static bool ShouldSkipPresentationShell(Unit? unit)
        {
            if (unit == null)
                return false;
            return MpVisualBudget.ShouldSkipPresentationComponent(unit);
        }

        internal static void TrySleepMissileRigidbody(Missile missile)
        {
            if (missile == null || missile.LocalSim || !MpSessionState.Active)
                return;

            if (!IsBeyondOpticalRange(missile) && !IsOffScreen(missile.transform.position))
                return;

            Rigidbody? rb = missile.rb;
            if (rb == null || rb.isKinematic)
                return;

            rb.isKinematic = true;
            rb.detectCollisions = false;
        }

        private static float ComputeOpticalRangeM()
        {
            float maxRange = 0f;
            if (GameManager.GetLocalAircraft(out Aircraft local) && local != null)
            {
                TargetDetector[] detectors = local.GetComponentsInChildren<TargetDetector>(true);
                for (int i = 0; i < detectors.Length; i++)
                {
                    TargetDetector detector = detectors[i];
                    if (detector == null || !detector.IsOperational())
                        continue;

                    float range = detector.GetVisualRange() * detector.GetVisualMagnification();
                    if (range > maxRange)
                        maxRange = range;
                }
            }

            if (maxRange <= 0f)
                maxRange = MpConfig.OpticalRangeFallbackM;

            if (maxRange > MpConfig.OpticalRangeCapM)
                maxRange = MpConfig.OpticalRangeCapM;

            return maxRange;
        }
    }
}
