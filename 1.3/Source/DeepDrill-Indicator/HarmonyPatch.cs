using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;

[HarmonyLib.HarmonyPatch(typeof(DeepResourceGrid), "DeepResourcesOnGUI")]
[StaticConstructorOnStartup]
public class HarmonyPatch
{
    static HarmonyPatch()
    {
        Harmony harmony = new Harmony("georodin.deepdrill");

        harmony.PatchAll();
    }

    static bool Prefix(Map ___map, ushort[] ___defGrid)
    {
        Thing singleSelectedThing = Find.Selector.SingleSelectedThing;
        if (singleSelectedThing != null)
        {
            CompDeepScanner compDeepScanner = singleSelectedThing.TryGetComp<CompDeepScanner>();
            CompDeepDrill compDeepDrill = singleSelectedThing.TryGetComp<CompDeepDrill>();

            if (((compDeepScanner != null || compDeepDrill != null)||singleSelectedThing.def.defName.Contains("autodrill")) && ___map.deepResourceGrid.AnyActiveDeepScannersOnMap())
            {
                int i = 0;
                foreach (ushort entry in ___defGrid)
                {
                    if (entry != 0)
                    {
                        IntVec3 c = ___map.cellIndices.IndexToCell(i);
                        if (!c.InBounds(___map))
                        {
                            return true;
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
                                if (Find.CameraDriver.CurrentZoom==CameraZoomRange.Closest) {
                                    GUI.color = Color.white;
                                    Text.Font = GameFont.Small;
                                    Text.Anchor = TextAnchor.MiddleLeft;
                                    Widgets.Label(new Rect(rect.xMax + 4f, rect.y, 999f, 29f), num+"");
                                    Text.Anchor = TextAnchor.UpperLeft;
                                }
                            }
                        }
                    }
                    i++;
                }
            }
        }

        return false;
    }
}