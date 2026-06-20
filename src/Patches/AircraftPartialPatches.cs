namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class AircraftPartialPatches
    {
        public static bool FixedUpdatePrefixSkip(Aircraft __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            if (!MpPatchGuard.IsPresentationUnit(__instance))
                return true;

            if (__instance.countermeasureTrigger)
                __instance.countermeasureManager.DeployCountermeasure(__instance);

            return false;
        }

        public static bool ShakeAircraftPrefixSkip(Aircraft __instance, float lowFreqShake, float highFreqShake)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            if (!MpPatchGuard.IsPresentationUnit(__instance))
                return true;

            return false;
        }
    }
}
