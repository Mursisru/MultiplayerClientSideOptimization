using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class ControlSurfaceJobPatches
    {
        public static bool UpdateJobFieldsPrefixSkip(ControlSurface __instance)
        {
            if (!MpSessionState.Active || __instance == null)
                return true;

            Aircraft? aircraft = __instance.GetComponentInParent<Aircraft>();
            if (aircraft == null || !MpPatchGuard.IsPresentationUnit(aircraft))
                return true;

            if (!MpVisualBudget.ShouldSkipPresentationComponent(aircraft))
                return true;

            return false;
        }
    }
}
