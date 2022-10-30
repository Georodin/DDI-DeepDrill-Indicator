using System.Collections.Generic;
using Verse;
using UnityEngine;

namespace DeepDrill_Indicator
{
    public class Settings : ModSettings
    {
        public static float sliderValueDDI = 27f;
        public override void ExposeData()
        {
            Scribe_Values.Look(ref sliderValueDDI, "sliderValueDDI");
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
            listingStandard.Label("DeepDrillIconSize".Translate()+Mathf.FloorToInt(Settings.sliderValueDDI));
            Settings.sliderValueDDI = listingStandard.Slider(DeepDrill_Indicator.Settings.sliderValueDDI, 6f, 80f);
            listingStandard.End();
            base.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "DeepDrillIndicator".Translate();
        }
    }
}
