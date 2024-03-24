using HarmonyLib;
using Verse;
using RimWorld;
using UnityEngine;
using DeepDrill_Indicator;
using static System.Collections.Specialized.BitVector32;

[HarmonyPatch(typeof(DeepResourceGrid), "SetAt")]
public class HPatchDeepResourceGridSetAt
{
    static bool Prefix(IntVec3 c, ThingDef def, int count)
    {
        Controller.ShouldUpdate(c);
        return true;
    }
}