﻿using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

[HarmonyPatch(typeof(DeepResourceGrid), "DeepResourcesOnGUI")]
[StaticConstructorOnStartup]
public class DDIHarmonyPatch
{

    static bool flagPerformance = false;
    static int counter = 0;

    static DDIHarmonyPatch()
    {
        Harmony harmony = new Harmony("georodin.deepdrill");

        harmony.PatchAll();
    }

    static bool Prefix(Map ___map, ushort[] ___defGrid)
    {
        if (IsDeepMiningRelated(___map))
        {
            if (!flagPerformance)
            {
                //is Scanner running?
                if (!___map.deepResourceGrid.AnyActiveDeepScannersOnMap())
                {
                    return false;
                }
                else
                {
                    flagPerformance = true;
                    counter = 0;
                }

            }

            int i = 0;
            foreach (ushort entry in ___defGrid)
            {
                if (entry != 0)
                {
                    IntVec3 c = ___map.cellIndices.IndexToCell(i);
                    if (!c.InBounds(___map))
                    {
                        return false;
                    }
                    ThingDef thingDef = ___map.deepResourceGrid.ThingDefAt(c);
                    if (thingDef != null)
                    {
                        int num = ___map.deepResourceGrid.CountAt(c);
                        if (num > 0)
                        {
                            Vector2 vector = c.ToVector3().MapToUIPosition();

                            float num2 = (UI.CurUICellSize() - DeepDrill_Indicator.Settings.sliderValueDDI) / 2f;
                            Rect rect = new Rect(vector.x + num2, vector.y - UI.CurUICellSize() + num2, DeepDrill_Indicator.Settings.sliderValueDDI, DeepDrill_Indicator.Settings.sliderValueDDI);

                            Widgets.ThingIcon(rect, thingDef);

                            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
                            {
                                GUI.color = Color.white;
                                Text.Font = GameFont.Small;
                                Text.Anchor = TextAnchor.MiddleCenter;

                                Widgets.Label(rect, num.ToString());

                                Text.Anchor = TextAnchor.UpperLeft; // Resetting the text anchor
                            }
                        }
                    }
                }
                i++;
            }
        }

        if (flagPerformance)
        {
            counter++;
            if (counter >= 60)
            {
                flagPerformance = false;
            }
        }

        return false;
    }

    static bool IsDeepMiningRelated(Map ___map)
    {
        if (flagPerformance)
        {
            return true;
        }

        if (IsSelectedDeepMiningRelated())
        {
            return true;
        }

        if (IsDesignatorDeepMiningRelated())
        {
            return true;
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