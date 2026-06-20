using System.IO;
using BepInEx;
using HarmonyLib;
using NOLoader.ModConfig;
using NOLoader.MultiplayerClientSideOptimization;

namespace MultiplayerClientSideOptimization.BepInEx
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public sealed class MpOptPlugin : BaseUnityPlugin
    {
        public const string PluginGuid = "com.at747.multiplayerclientsideoptimization";
        public const string PluginName = "Multiplayer Client-Side Optimization";
        public const string PluginVersion = "0.6.0";

        private Harmony? _harmony;
        private float _profilerAccum;
        private int _deoptFrameCounter;

        private void Awake()
        {
            string pluginDir = Path.Combine(Paths.PluginPath, PluginGuid);
            Directory.CreateDirectory(pluginDir);
            string iniPath = Path.Combine(pluginDir, "mod.ini");
            if (!File.Exists(iniPath))
                File.WriteAllText(iniPath, DefaultModIni);

            MpConfig.Load(ModIniConfig.Load(pluginDir, "mod.ini"));
            MpSessionState.RefreshDedicatedClientGate();

            if (MpSessionState.DisabledBecauseHost)
            {
                Logger.LogInfo("[MpOpt] host/server detected — mod inactive (dedicated client only)");
                return;
            }

            _harmony = new Harmony(PluginGuid);
            PatchBootstrap.Apply(_harmony);
            Logger.LogInfo("[MpOpt] active dedicated-client optimization v0.6.0 rbThrottle="
                + (MpConfig.RbMoveThrottleEnabled ? "1" : "0")
                + " fullStride=" + MpConfig.VisualFullZoneStride
                + " visualStride=" + MpConfig.VisualUpdateStride
                + " fullM=" + MpConfig.PresentationFullM
                + " memMb=" + MpConfig.MemoryReservoirMb);
        }

        private void OnDestroy()
        {
            MpDeepFreezeManager.RestoreAll();
            _harmony?.UnpatchSelf();
            MpMemoryBudget.Release();
            MpBufferUnitMap.Clear();
            MpSessionState.Deactivate();
        }

        private void Update()
        {
            MpSessionState.RefreshDedicatedClientGate();
            if (!MpSessionState.Active)
                return;

            MpMemoryBudget.TryApplyOnce();

            float dt = UnityEngine.Time.unscaledDeltaTime;
            MpDeepFreezeManager.TickScan(dt);

            _deoptFrameCounter++;
            if (_deoptFrameCounter % MpConfig.DeepFreezeDeoptStride == 0)
                MpDeepFreezeManager.TickDeopt();

            if (!MpConfig.ProfilerEnabled)
                return;

            _profilerAccum += dt;
            if (_profilerAccum < MpConfig.ReportIntervalS)
                return;

            _profilerAccum = 0f;
            MpPhysicsProfilerBepInEx.LogSnapshot(Logger);
        }

        private const string DefaultModIni = @"[MpOpt]
profiler=1
report_interval_s=5
optical_range_m=25000
optical_range_cap_m=12000
presentation_far_m=4000
presentation_near_m=1000
presentation_full_m=600
fx_max_distance_m=2000
rb_move_throttle=1
rb_move_stride=4
rb_physics_sleep_m=5000
visual_update_stride=3
visual_mid_onscreen_stride=1
component_update_stride=2
memory_budget=1
memory_reservoir_mb=4096
asset_warm=1
texture_mipmap_limit=0
lod_bias_min=1.5
grass_position_buffer_percent=1
grass_visible_buffer_percent=0.5
deep_freeze=1
deep_freeze_min_m=40000
deep_freeze_scan_interval_s=1
deep_freeze_deopt_stride=2
incoming_missile_max_m=80000
incoming_missile_dot_min=0.65
deep_freeze_disable_lights=1
deep_freeze_disable_renderers=0
";
    }
}
