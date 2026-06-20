namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpMotionBudget
    {
        private static float _localSpeedMps;

        internal static void Refresh()
        {
            if (!MpConfig.MotionStrideEnabled || !MpSessionState.Active)
            {
                _localSpeedMps = 0f;
                return;
            }

            if (GameManager.GetLocalAircraft(out Aircraft local) && local != null)
                _localSpeedMps = local.speed;
            else
                _localSpeedMps = 0f;
        }

        internal static float GetEffectivePresentationFullM()
        {
            if (!IsMotionActive())
                return MpConfig.PresentationFullM;

            return MpConfig.PresentationFullM * MpConfig.MotionZoneScale;
        }

        internal static float GetEffectivePresentationNearM()
        {
            if (!IsMotionActive())
                return MpConfig.PresentationNearM;

            return MpConfig.PresentationNearM * MpConfig.MotionZoneScale;
        }

        private static bool IsMotionActive() =>
            MpConfig.MotionStrideEnabled && _localSpeedMps >= MpConfig.MotionSpeedThresholdMps;
    }
}
