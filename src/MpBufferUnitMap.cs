using System.Collections.Generic;
using NuclearOption.NetworkTransforms;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpBufferUnitMap
    {
        private static readonly object Sync = new object();
        private static readonly Dictionary<ISnapshotBuffer, Unit> Map = new Dictionary<ISnapshotBuffer, Unit>();

        internal static void Register(ISnapshotBuffer? buffer, Unit? unit)
        {
            if (buffer == null || unit == null)
                return;

            lock (Sync)
            {
                Map[buffer] = unit;
            }
        }

        internal static bool TryGetUnit(ISnapshotBuffer? buffer, out Unit? unit)
        {
            unit = null;
            if (buffer == null)
                return false;

            lock (Sync)
            {
                return Map.TryGetValue(buffer, out unit);
            }
        }

        internal static void Clear()
        {
            lock (Sync)
            {
                Map.Clear();
            }
        }
    }
}
