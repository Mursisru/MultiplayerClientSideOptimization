namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpSessionState
    {
        internal static bool Active { get; private set; }
        internal static bool DisabledBecauseHost { get; private set; }

        internal static void RefreshDedicatedClientGate()
        {
            if (MpPatchGuard.IsHostOrServerActive())
            {
                Active = false;
                DisabledBecauseHost = true;
                return;
            }

            DisabledBecauseHost = false;
            Active = true;
        }

        internal static void Deactivate()
        {
            Active = false;
        }
    }
}
