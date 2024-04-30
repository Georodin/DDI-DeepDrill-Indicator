using Verse;
using UnityEngine;

namespace DeepDrill_Indicator
{
    public class DDISettings : ModSettings
    {
        // Singleton instance
        private static DDISettings instance;

        // Public property to access the singleton instance
        public static DDISettings Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DDISettings();
                }
                return instance;
            }
        }

        // Private constructor to enforce singleton usage
        private DDISettings() { }

        // Settings fields
        public float SwitchThreshold = 20.0f;
        public float ScaleFarIndicator = 5.0f;
        public bool DisableSteelOverview = false;

        public override void ExposeData()
        {
            Scribe_Values.Look(ref SwitchThreshold, "DDISwitchThreshold", 20.0f);
            Scribe_Values.Look(ref ScaleFarIndicator, "DDIScaleFarIndicator", 5.0f);
            Scribe_Values.Look(ref DisableSteelOverview, "DDIDisableSteelOverview", false);
        }
    }

    public class DeepDrill_IndicatorMod : Mod
    {

        public DeepDrill_IndicatorMod(ModContentPack content) : base(content)
        {
            
        }
        public override void DoSettingsWindowContents(Rect inRect)
        {
            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.Begin(inRect);

            listingStandard.Label($"{"SwitchThreshold".Translate()}: {DDISettings.Instance.SwitchThreshold:F2}");
            DDISettings.Instance.SwitchThreshold = listingStandard.Slider(DDISettings.Instance.SwitchThreshold, 0.0f, 200.0f);
            if (Current.Game != null)
            {
                listingStandard.Label($"{"CurrentZoomDistance".Translate()}: {Find.CameraDriver.ZoomRootSize:F2}\n");
            }

            listingStandard.Label($"{"ScaleFarIndicator".Translate()}: {DDISettings.Instance.ScaleFarIndicator:F2}");
            DDISettings.Instance.ScaleFarIndicator = listingStandard.Slider(DDISettings.Instance.ScaleFarIndicator, 0.0f, 25.0f);

            // Add a checkbox for the new setting
            listingStandard.CheckboxLabeled("DisableSteelOverview".Translate(), ref DDISettings.Instance.DisableSteelOverview);

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
