using System.Collections.Generic;
using System.Reflection;
using Mirage;
using NuclearOption.DebugScripts;
using NuclearOption.NetworkTransforms;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class SendTransformBatcherRedirect
    {
        private static FieldInfo? _cameraField;
        private static FieldInfo? _cameraPositionField;
        private static FieldInfo? _serverField;
        private static FieldInfo? _aircraftListField;
        private static FieldInfo? _otherListField;
        private static FieldInfo? _smoothTimeField;
        private static FieldInfo? _currentTimeValuesField;
        private static FieldInfo? _snapTimeField;
        private static FieldInfo? _debuggerField;
        private static bool _reflectionReady;

        public static bool VisualUpdatePrefixSkip(SendTransformBatcher batcher)
        {
            if (batcher == null)
                return true;

            if (!MpSessionState.Active || MpPatchGuard.IsHostOrServerActive())
                return true;

            VisualUpdateRedirect(batcher);
            return false;
        }

        public static void VisualUpdateRedirect(SendTransformBatcher batcher)
        {
            if (batcher == null)
                return;

            if (!EnsureReflection())
            {
                return;
            }

            Camera? camera = _cameraField!.GetValue(batcher) as Camera;
            if (camera == null)
            {
                camera = Camera.main;
                _cameraField.SetValue(batcher, camera);
            }

            if (camera == null)
                return;

            Vector3 cameraPosition = camera.transform.position;
            _cameraPositionField!.SetValue(batcher, cameraPosition);

            var server = _serverField!.GetValue(batcher) as NetworkServer;
            bool serverActive = server != null && server.Active;

            var smoothTime = _smoothTimeField!.GetValue(batcher) as SmoothNetworkTime;
            if (smoothTime == null)
                return;

            double interpolationTime = smoothTime.InterpolationTime;
            float maxExtrapolation = batcher.MaxExtrapolation;
            double extrapolationOffset = smoothTime.GetExtrapolationOffset(maxExtrapolation);

            object? currentTimeValues = _currentTimeValuesField!.GetValue(batcher);
            float maxSnapshotAge = GetMaxSnapshotAge(currentTimeValues);
            bool snap = _snapTimeField != null && _snapTimeField.GetValue(batcher) is bool snapFlag && snapFlag;

            var visualUpdateTime = new VisualUpdateTime
            {
                interpolationTime = interpolationTime,
                extrapolationOffset = extrapolationOffset,
                maxExtrapolateAge = maxSnapshotAge,
                snap = snap
            };

            bool useBudget = MpSessionState.Active && !MpPatchGuard.IsHostOrServerActive();

            var aircraftList = _aircraftListField!.GetValue(batcher) as List<AircraftNetworkTransform>;
            if (aircraftList != null)
            {
                for (int i = 0; i < aircraftList.Count; i++)
                {
                    AircraftNetworkTransform nt = aircraftList[i];
                    if (serverActive && nt == null)
                        continue;

                    if (useBudget && TrySkipTransform(nt, out Unit? unit))
                    {
                        continue;
                    }

                    nt!.VisualUpdate(ref visualUpdateTime);
                    double removeBefore = interpolationTime - nt.SyncInterval * 8.0;
                    nt.SnapshotBuffer.RemoveOld(removeBefore);
                }
            }

            var otherList = _otherListField!.GetValue(batcher) as List<NetworkTransformBase>;
            if (otherList != null)
            {
                for (int i = 0; i < otherList.Count; i++)
                {
                    NetworkTransformBase nt = otherList[i];
                    if (nt == null)
                        continue;

                    if (useBudget && TrySkipTransform(nt, out Unit? unit))
                    {
                        continue;
                    }

                    nt.VisualUpdate(ref visualUpdateTime);
                    double removeBefore = interpolationTime - nt.SyncInterval * 8.0;
                    nt.SnapshotBuffer.RemoveOld(removeBefore);
                }
            }

            if (DebugVis.Enabled && SceneSingleton<CameraStateManager>.i != null && _debuggerField != null)
            {
                var debugger = _debuggerField.GetValue(batcher) as SendTransformBatcherDebugger;
                debugger?.UpdateDebugFollow(ref visualUpdateTime);
            }
        }

        private static bool TrySkipTransform(NetworkTransformBase nt, out Unit? unit)
        {
            unit = null;
            if (!MpNetworkTransformHelper.TryGetUnit(nt, out unit) || unit == null)
                return false;

            MpBufferUnitMap.Register(nt.SnapshotBuffer, unit);

            if (MpDeepFreezeManager.IsFrozen(unit))
            {
                return true;
            }

            if (!MpVisualBudget.ShouldSkipVisualUpdate(unit, unit.transform.position))
                return false;

            return true;
        }

        private static float GetMaxSnapshotAge(object? currentTimeValues)
        {
            if (currentTimeValues == null)
                return 0f;

            FieldInfo? field = currentTimeValues.GetType().GetField(
                "MaxSnapshotAge",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (field == null)
                return 0f;

            object? value = field.GetValue(currentTimeValues);
            return value is float f ? f : 0f;
        }

        private static bool EnsureReflection()
        {
            if (_reflectionReady)
                return true;

            const BindingFlags inst = BindingFlags.Instance | BindingFlags.NonPublic;
            var t = typeof(SendTransformBatcher);

            _cameraField = t.GetField("camera", inst);
            _cameraPositionField = t.GetField("cameraPosition", inst);
            _serverField = t.GetField("Server", inst);
            _aircraftListField = t.GetField("_behavioursClientAircraft", inst);
            _otherListField = t.GetField("_behavioursClientOther", inst);
            _smoothTimeField = t.GetField("smoothTime", inst);
            _currentTimeValuesField = t.GetField("currentTimeValues", inst);
            _snapTimeField = t.GetField("snapTime", inst);
            _debuggerField = t.GetField("debugger", inst);

            _reflectionReady = _cameraField != null
                && _cameraPositionField != null
                && _serverField != null
                && _aircraftListField != null
                && _otherListField != null
                && _smoothTimeField != null
                && _currentTimeValuesField != null
                && _snapTimeField != null;

            return _reflectionReady;
        }
    }
}
