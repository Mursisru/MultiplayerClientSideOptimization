using NOLoader.ModConfig;

namespace NOLoader.MultiplayerClientSideOptimization
{
    internal static class MpConfig
    {
        internal const string Section = "MpOpt";

        internal static bool ProfilerEnabled { get; private set; } = true;
        internal static int ReportIntervalS { get; private set; } = 5;
        internal static float OpticalRangeFallbackM { get; private set; } = 25000f;
        internal static float OpticalRangeCapM { get; private set; } = 12000f;
        internal static float PresentationFarM { get; private set; } = 4000f;
        internal static float PresentationNearM { get; private set; } = 800f;
        internal static float PresentationFullM { get; private set; } = 400f;
        internal static int ComponentUpdateStride { get; private set; } = 3;
        internal static int VisualUpdateStride { get; private set; } = 4;
        internal static float FxMaxDistanceM { get; private set; } = 1500f;
        internal static bool RbMoveThrottleEnabled { get; private set; } = true;
        internal static int RbMoveStride { get; private set; } = 4;
        internal static float RbPhysicsSleepM { get; private set; } = 3000f;
        internal static bool VisualMidOnscreenStride { get; private set; } = true;
        internal static bool VisualFullOffscreenSkip { get; private set; } = true;
        internal static int VisualFullZoneStride { get; private set; } = 2;
        internal static bool ComponentFullOffscreenSkip { get; private set; } = true;

        internal static bool MemoryBudgetEnabled { get; private set; } = true;
        internal static int MemoryReservoirMb { get; private set; } = 4096;
        internal static bool AssetWarmEnabled { get; private set; } = true;
        internal static int TextureMipmapLimit { get; private set; }
        internal static float LodBiasMin { get; private set; } = 1.5f;
        internal static float GrassPositionBufferPercent { get; private set; } = 1f;
        internal static float GrassVisibleBufferPercent { get; private set; } = 0.5f;

        internal static bool DeepFreezeEnabled { get; private set; } = true;
        internal static float DeepFreezeMinM { get; private set; } = 40000f;
        internal static float DeepFreezeScanIntervalS { get; private set; } = 1f;
        internal static int DeepFreezeDeoptStride { get; private set; } = 2;
        internal static float IncomingMissileMaxM { get; private set; } = 80000f;
        internal static float IncomingMissileDotMin { get; private set; } = 0.65f;
        internal static bool DeepFreezeDisableLights { get; private set; } = true;
        internal static bool DeepFreezeDisableRenderers { get; private set; }

        internal static void Load(ModIniConfig cfg)
        {
            ProfilerEnabled = cfg.GetBool(Section, "profiler", true);
            ReportIntervalS = cfg.GetInt(Section, "report_interval_s", 5);
            OpticalRangeFallbackM = cfg.GetFloat(Section, "optical_range_m", 25000f);
            OpticalRangeCapM = cfg.GetFloat(Section, "optical_range_cap_m", 12000f);
            PresentationFarM = cfg.GetFloat(Section, "presentation_far_m", 4000f);
            PresentationNearM = cfg.GetFloat(Section, "presentation_near_m", 800f);
            PresentationFullM = cfg.GetFloat(Section, "presentation_full_m", 400f);
            ComponentUpdateStride = cfg.GetInt(Section, "component_update_stride", 3);
            if (ComponentUpdateStride < 2)
                ComponentUpdateStride = 2;
            FxMaxDistanceM = cfg.GetFloat(Section, "fx_max_distance_m", 1500f);
            RbMoveThrottleEnabled = cfg.GetBool(Section, "rb_move_throttle", true);
            RbMoveStride = cfg.GetInt(Section, "rb_move_stride", 4);
            if (RbMoveStride < 2)
                RbMoveStride = 2;
            RbPhysicsSleepM = cfg.GetFloat(Section, "rb_physics_sleep_m", 3000f);
            VisualUpdateStride = cfg.GetInt(Section, "visual_update_stride", 4);
            if (VisualUpdateStride < 2)
                VisualUpdateStride = 2;
            VisualMidOnscreenStride = cfg.GetBool(Section, "visual_mid_onscreen_stride", true);
            VisualFullOffscreenSkip = cfg.GetBool(Section, "visual_full_offscreen_skip", true);
            VisualFullZoneStride = cfg.GetInt(Section, "visual_full_zone_stride", 2);
            if (VisualFullZoneStride < 1)
                VisualFullZoneStride = 1;
            ComponentFullOffscreenSkip = cfg.GetBool(Section, "component_full_offscreen_skip", true);

            MemoryBudgetEnabled = cfg.GetBool(Section, "memory_budget", true);
            MemoryReservoirMb = cfg.GetInt(Section, "memory_reservoir_mb", 4096);
            if (MemoryReservoirMb < 0)
                MemoryReservoirMb = 0;
            if (MemoryReservoirMb > 8192)
                MemoryReservoirMb = 8192;
            AssetWarmEnabled = cfg.GetBool(Section, "asset_warm", true);
            TextureMipmapLimit = cfg.GetInt(Section, "texture_mipmap_limit", 0);
            if (TextureMipmapLimit < 0)
                TextureMipmapLimit = 0;
            if (TextureMipmapLimit > 3)
                TextureMipmapLimit = 3;
            LodBiasMin = cfg.GetFloat(Section, "lod_bias_min", 1.5f);
            GrassPositionBufferPercent = cfg.GetFloat(Section, "grass_position_buffer_percent", 1f);
            GrassVisibleBufferPercent = cfg.GetFloat(Section, "grass_visible_buffer_percent", 0.5f);

            DeepFreezeEnabled = cfg.GetBool(Section, "deep_freeze", true);
            DeepFreezeMinM = cfg.GetFloat(Section, "deep_freeze_min_m", 40000f);
            DeepFreezeScanIntervalS = cfg.GetFloat(Section, "deep_freeze_scan_interval_s", 1f);
            if (DeepFreezeScanIntervalS < 0.25f)
                DeepFreezeScanIntervalS = 0.25f;
            DeepFreezeDeoptStride = cfg.GetInt(Section, "deep_freeze_deopt_stride", 2);
            if (DeepFreezeDeoptStride < 1)
                DeepFreezeDeoptStride = 1;
            IncomingMissileMaxM = cfg.GetFloat(Section, "incoming_missile_max_m", 80000f);
            IncomingMissileDotMin = cfg.GetFloat(Section, "incoming_missile_dot_min", 0.65f);
            DeepFreezeDisableLights = cfg.GetBool(Section, "deep_freeze_disable_lights", true);
            DeepFreezeDisableRenderers = cfg.GetBool(Section, "deep_freeze_disable_renderers", false);
        }
    }
}
