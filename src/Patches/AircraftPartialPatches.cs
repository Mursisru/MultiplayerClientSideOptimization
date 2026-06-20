namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class AircraftPartialPatches
    {
        public static bool FixedUpdatePrefixSkip(Aircraft __instance)
        {
            return true;
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
