using Verse;
using UnityEngine;

namespace DeepDrill_Indicator
{
    public class DDISettings : ModSettings
    {
        // Settings fields
        public static float SwitchThreshold = 20.0f;
        public static float ScaleFarIndicator = 5.0f;
        public static bool DisableSteelOverview = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref SwitchThreshold, "DDISwitchThreshold", 20.0f);
            Scribe_Values.Look(ref ScaleFarIndicator, "DDIScaleFarIndicator", 5.0f);
            Scribe_Values.Look(ref DisableSteelOverview, "DDIDisableSteelOverview", false);
        }
    }

    public class DeepDrill_IndicatorMod : Mod
    {
        DDISettings settings;
        public DeepDrill_IndicatorMod(ModContentPack content) : base(content)
        {
            settings = GetSettings<DDISettings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label((TaggedString)$"{"SwitchThreshold".Translate()}: {DDISettings.SwitchThreshold:F2}");
            DDISettings.SwitchThreshold = listingStandard.Slider(DDISettings.SwitchThreshold, 0.0f, 200.0f);

            if (Current.Game != null)
            {
                listingStandard.Label((TaggedString)$"{"CurrentZoomDistance".Translate()}: {Find.CameraDriver.ZoomRootSize:F2}\n");
            }

            listingStandard.Label((TaggedString)$"{"ScaleFarIndicator".Translate()}: {DDISettings.ScaleFarIndicator:F2}");
            DDISettings.ScaleFarIndicator = listingStandard.Slider(DDISettings.ScaleFarIndicator, 0.0f, 25.0f);

            listingStandard.CheckboxLabeled("DisableSteelOverview".Translate(), ref DDISettings.DisableSteelOverview);

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
            if (Current.Game != null)
            {
                Find.CurrentMap.GetComponent<GridController>().ShouldUpdate(IntVec3.Zero, true);
            }
        }
    }
}