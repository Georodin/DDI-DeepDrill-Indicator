using HarmonyLib;
using Verse;
using RimWorld;
using DeepDrill_Indicator;

[HarmonyPatch(typeof(DeepResourceGrid), "DeepResourcesOnGUI")]
public class HPatchDeepResourceGrid
{
    static bool Prefix(Map ___map, ushort[] ___defGrid, ushort[] ___countGrid)
    {
        bool related = IsDeepMiningRelated(___map);

        DeepResourcesCloseSectionLayer.isVisible = related;
        DeepResourcesFarSectionLayer.isVisible = related;

        /*
        try
        {
            // Define file paths
            string defGridPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "DefGrid.txt");
            string countGridPath = Path.Combine(GenFilePaths.SaveDataFolderPath, "CountGrid.txt");

            // Save ___defGrid to DefGrid.txt
            File.WriteAllLines(defGridPath, ___defGrid.Select(d => d.ToString()));
            Log.Message($"DefGrid saved to: {defGridPath}");

            // Save ___countGrid to CountGrid.txt
            File.WriteAllLines(countGridPath, ___countGrid.Select(c => c.ToString()));
            Log.Message($"CountGrid saved to: {countGridPath}");
        }
        catch (Exception ex)
        {
            Log.Error($"Failed to save grids: {ex.Message}");
        }
        */

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