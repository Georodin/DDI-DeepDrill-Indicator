using HarmonyLib;
using RimWorld;
using Verse;

namespace DeepDrill_Indicator
{
    [StaticConstructorOnStartup]
    public class HarmonyPatcher
    {
        static HarmonyPatcher()
        {
            Harmony harmony = new Harmony("georodin.deepdrill");

            harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(DeepResourceGrid), "SetAt")]
    public class HPatchDeepResourceGridSetAt
    {
        static void Postfix(IntVec3 c, ThingDef def, int count, Map ___map)
        {
            ___map.GetComponent<GridController>().ShouldUpdate(c);
            ___map.GetComponent<ResourceUtil>().UpdateAt(c);
        }
    }

    [HarmonyPatch(typeof(CompDeepDrill), "TryProducePortion")]
    public class HPatchDeepResourceGridTryProducePortion
    {
        static void Postfix(ThingWithComps ___parent)
        {
            ___parent.Map.GetComponent<ResourceUtil>().UpdateGridFields();
            Log.Message("TryProducePortion");
        }
    }
    
    [HarmonyPatch(typeof(CompDeepScanner), "DoFind")]
    public class HPatchDeepResourceGridDoFind
    {
        static void Postfix(ThingWithComps ___parent)
        {
            ___parent.Map.GetComponent<ResourceUtil>().UpdateGridFields();
            Log.Message("DoFind");
        }
    }
}
