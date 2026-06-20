using NOLoader.API;
using NOLoader.API.World;
using NOLoader.ModConfig;
using UnityEngine;

namespace NOLoader.MultiplayerClientSideOptimization
{
    public sealed class MultiplayerClientSideOptimizationMod : INOMod, INOModTickSlow, INOModTickNormal
    {
        public void OnLoad(ref NOModContext ctx)
        {
            ModIniConfig.EnsureDefault(ctx.ModRoot, DefaultModIni, "mod.ini");
            MpConfig.Load(ModIniConfig.Load(ctx.ModRoot, "mod.ini"));
            MpSessionState.RefreshDedicatedClientGate();

            // #region agent log
            MpDebugTrace.Log(
                "H2",
                "MultiplayerClientSideOptimizationMod.cs:OnLoad",
                "mod loaded",
                "{\"modRoot\":\"" + MpDebugTrace.EscapePath(ctx.ModRoot) + "\",\"active\":" + MpDebugTrace.B(MpSessionState.Active)
                + ",\"disabledHost\":" + MpDebugTrace.B(MpSessionState.DisabledBecauseHost)
                + ",\"lodBias\":" + MpDebugTrace.F(QualitySettings.lodBias)
                + ",\"mipmapLimit\":" + MpDebugTrace.I(QualitySettings.globalTextureMipmapLimit)
                + ",\"memBudgetMb\":" + MpDebugTrace.I(MpConfig.MemoryReservoirMb)
                + ",\"assetWarm\":" + MpDebugTrace.B(MpConfig.AssetWarmEnabled)
                + ",\"deepFreeze\":" + MpDebugTrace.B(MpConfig.DeepFreezeEnabled)
                + ",\"lodBiasMin\":" + MpDebugTrace.F(MpConfig.LodBiasMin)
                + ",\"textureMipmapCfg\":" + MpDebugTrace.I(MpConfig.TextureMipmapLimit) + "}");
            // #endregion

            if (MpSessionState.DisabledBecauseHost)
            {
                LoaderLog.Write("[MpOpt] host/server detected — mod inactive (dedicated client only)");
                return;
            }

            INOModWorldReader world = NOModRuntime.ActivateWorld();
            ctx.Services.World = world;

            LoaderLog.Write("[MpOpt] active dedicated-client optimization v0.6.0 rbThrottle="
                + (MpConfig.RbMoveThrottleEnabled ? "1" : "0")
                + " fullStride=" + MpConfig.VisualFullZoneStride
                + " visualStride=" + MpConfig.VisualUpdateStride
                + " fullM=" + MpConfig.PresentationFullM
                + " memMb=" + MpConfig.MemoryReservoirMb);
        }

        public void OnUnload(ref NOModContext ctx)
        {
            MpDeepFreezeManager.RestoreAll();
            MpMemoryBudget.Release();
            MpBufferUnitMap.Clear();
            MpSessionState.Deactivate();
            LoaderLog.Write("[MpOpt] unloaded visual=" + MpStats.VisualUpdateSkipped
                + " visualFull=" + MpStats.VisualFullZoneSkipped
                + " removeOld=" + MpStats.SnapshotRemoveOldSkipped
                + " comp=" + MpStats.RemoteComponentSkipped
                + " weapon=" + MpStats.WeaponShellSkipped);
        }

        public void OnSlowUpdate(ref NOModContext ctx, float dt)
        {
            MpSessionState.RefreshDedicatedClientGate();
            if (!MpSessionState.Active)
                return;

            MpMemoryBudget.TryApplyOnce();
            MpDeepFreezeManager.TickScan(dt);
            MpPhysicsProfiler.ReportIfDue(dt);
        }

        public void OnNormalUpdate(ref NOModContext ctx, float dt)
        {
            MpSessionState.RefreshDedicatedClientGate();
            if (!MpSessionState.Active)
                return;

            MpDeepFreezeManager.TickDeopt();
        }

        private const string DefaultModIni = @"[MpOpt]
profiler=1
report_interval_s=5
optical_range_m=25000
optical_range_cap_m=12000
presentation_far_m=4000
presentation_near_m=800
presentation_full_m=400
fx_max_distance_m=1500
rb_move_throttle=1
rb_move_stride=4
rb_physics_sleep_m=3000
visual_update_stride=4
visual_full_offscreen_skip=1
visual_full_zone_stride=2
visual_mid_onscreen_stride=1
component_update_stride=3
component_full_offscreen_skip=1
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
