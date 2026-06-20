using NuclearOption.NetworkTransforms;

namespace NOLoader.MultiplayerClientSideOptimization.Patches
{
    internal static class SnapshotBufferPatches
    {
        public static bool RemoveOldPrefixSkip(object __instance, double timestamp)
        {
            if (!MpSessionState.Active || __instance is not ISnapshotBuffer buffer)
                return true;

            if (!MpBufferUnitMap.TryGetUnit(buffer, out Unit? unit) || unit == null)
                return true;

            if (!MpVisualBudget.ShouldSkipVisualUpdate(unit, unit.transform.position))
                return true;

            return false;
        }
    }
}
