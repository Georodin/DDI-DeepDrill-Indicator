using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
}
