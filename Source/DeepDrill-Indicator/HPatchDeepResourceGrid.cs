using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using DeepDrill_Indicator;
using static System.Collections.Specialized.BitVector32;

[HarmonyPatch(typeof(DeepResourceGrid), "DeepResourcesOnGUI")]
public class HPatchDeepResourceGrid
{
    static bool Prefix(Map ___map, ushort[] ___defGrid, ushort[] ___countGrid)
    {
        bool related = IsDeepMiningRelated(___map);

        DeepResourcesCloseSectionLayer.isVisible = related;
        DeepResourcesFarSectionLayer.isVisible = related;

        return false;
    }

    static bool IsDeepMiningRelated(Map ___map)
    {
        bool found = false;
        if (IsSelectedDeepMiningRelated())
        {
            found = true;
        }

        if (!found)
        {
            if (IsDesignatorDeepMiningRelated())
            {
                found = true;
            }
        }

        if(found)
        {
            if (___map.deepResourceGrid.AnyActiveDeepScannersOnMap())
            {
                return true;
            }
        }

        return false;
    }

    static bool IsSelectedDeepMiningRelated()
    {
        Thing singleSelectedThing = Find.Selector.SingleSelectedThing;

        if (singleSelectedThing != null)
        {
            //check for Scanner
            if (singleSelectedThing.TryGetComp<CompDeepScanner>() != null)
            {
                return true;
            }

            //check for Drill
            if (singleSelectedThing.TryGetComp<CompDeepDrill>() != null)
            {
                return true;
            }

            //check for minified Drill
            if (singleSelectedThing is MinifiedThing minified)
            {
                if (minified.InnerThing.TryGetComp<CompDeepDrill>() != null)
                {
                    return true;
                }
            }

            // Check for DeepDrill blueprint
            if (singleSelectedThing is Blueprint_Build blueprint)
            {
                if (blueprint.def.entityDefToBuild == ThingDefOf.DeepDrill)
                {
                    return true;
                }
            }

            // Check for AutoDrill from Mod "AutoDrillReUpload 1.4" id=2884084876
            if (singleSelectedThing.def.defName.Contains("autodrill"))
            {
                return true;
            }
        }

        return false;
    }

    static bool IsDesignatorDeepMiningRelated()
    {
        //check if drill is selected as building blueprint
        Designator currentDesignator = Find.DesignatorManager.SelectedDesignator;
        if (currentDesignator is Designator_Build designatorBuild)
        {
            BuildableDef buildable = designatorBuild.PlacingDef as BuildableDef;

            if (buildable != null && buildable is ThingDef thingDef && thingDef == ThingDefOf.DeepDrill)
            {
                return true;
            }

            //check for AutoDrill from Mod "AutoDrillReUpload 1.4" id=2884084876
            if (buildable.defName.Contains("autodrill"))
            {
                return true;
            }
        }

        return false;
    }
}