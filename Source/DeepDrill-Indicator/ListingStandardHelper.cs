﻿using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace DeepDrill_Indicator
{
    public class Settings : ModSettings
    {
        public static float SwitchThreshold = 20.0f;

        public static float ScaleFarIndicator = 5.0f;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SwitchThreshold, "DDISwitchThreshold", 20.0f);
            Scribe_Values.Look(ref ScaleFarIndicator, "DDIScaleFarIndicator", 5.0f);
        }
    }

    public class DeepDrill_IndicatorMod : Mod
    {
        Settings settings;

        public DeepDrill_IndicatorMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label($"{"SwitchThreshold".Translate()}: {Settings.SwitchThreshold:F2}");
            Settings.SwitchThreshold = listingStandard.Slider(Settings.SwitchThreshold, 0.0f, 200.0f);
            listingStandard.Label($"{"CurrentZoomDistance".Translate()}: {Find.CameraDriver.ZoomRootSize:F2}\n");

            listingStandard.Label($"{"ScaleFarIndicator".Translate()}: {Settings.ScaleFarIndicator:F2}");
            Settings.ScaleFarIndicator = listingStandard.Slider(Settings.ScaleFarIndicator, 0.0f, 25.0f);

            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "DeepDrillIndicator".Translate();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            Controller.ShouldUpdate(IntVec3.Zero, true);
        }

    }
}
