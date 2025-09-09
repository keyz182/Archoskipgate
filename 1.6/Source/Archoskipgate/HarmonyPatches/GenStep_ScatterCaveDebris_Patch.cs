using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Archoskipgate.HarmonyPatches;

[HarmonyPatch(typeof(RimWorld.GenStep_ScatterCaveDebris))]
public static class GenStep_ScatterCaveDebris
{

    [HarmonyPatch(nameof(RimWorld.GenStep_ScatterCaveDebris.Generate))]
    [HarmonyPatch("Generate")]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        List<CodeInstruction> codes = new List<CodeInstruction>(instructions);

        for (int i = 0; i < codes.Count; i++)
        {
            if (i + 2 < codes.Count &&
                codes[i].opcode == OpCodes.Callvirt &&
                codes[i].operand is MethodInfo methodInfo &&
                methodInfo.Name == "HasCaves")
            {
                ModLog.Debug("Found methods to patch for GenStep_CavesTerrain.Generate");
                // Remove the World.HasCaves call and its setup
                yield return new CodeInstruction(OpCodes.Pop);  // Remove the Tile parameter
                yield return new CodeInstruction(OpCodes.Pop);  // Remove the World instance

                // Load map parameter for our method
                yield return new CodeInstruction(OpCodes.Ldarg_1);

                // Call our custom method instead
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(Utils), nameof(Utils.IsPocketMapWithCaves)));
            }
            else
            {
                yield return codes[i];
            }
        }
    }

}
