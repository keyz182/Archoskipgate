using HarmonyLib;
using RimWorld;
using Verse;

namespace Archoskipgate.HarmonyPatches;

[HarmonyPatch(typeof(FloatMenuOptionProvider_EnterMapPortal))]
public static class FloatMenuOptionProvider_EnterMapPortal_Patch
{
    [HarmonyPatch("GetSingleOptionFor")]
    [HarmonyPrefix]
    public static bool GetSingleOptionFor_Prefix(Thing clickedThing)
    {
        if (clickedThing is not Building_SkipGate)
            return true;

        return false;
    }

}
