using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Archoskipgate.HarmonyPatches;

[HarmonyPatch(typeof(RimWorld.GenStep_Caves))]
public static class GenStep_Caves_Generate_Patch
{
    public static bool IsPocketMapWithCaves(Map map)
    {
        if (map.IsPocketMap && map.pocketTileInfo == null) return false;

        float chance;
        if (map.TileInfo.hilliness >= Hilliness.Mountainous)
        {
            chance = 0.5f;
        }
        else
        {
            if (map.TileInfo.hilliness < Hilliness.LargeHills)
                return false;
            chance = 0.25f;
        }
        return Rand.ChanceSeeded(chance, Gen.HashCombineInt(Find.World.info.Seed, map.IsPocketMap ? GateAddress.CurrentGateAddress.address.GetHashCode() : map.Tile));
    }

    [HarmonyPatch(nameof(RimWorld.GenStep_Caves.Generate))]
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
                ModLog.Debug("Found methods to patch for GenStep_Caves.Generate");
                // Remove the World.HasCaves call and its setup
                yield return new CodeInstruction(OpCodes.Pop);  // Remove the Tile parameter
                yield return new CodeInstruction(OpCodes.Pop);  // Remove the World instance

                // Load map parameter for our method
                yield return new CodeInstruction(OpCodes.Ldarg_1);

                // Call our custom method instead
                yield return new CodeInstruction(OpCodes.Call,
                    AccessTools.Method(typeof(GenStep_Caves_Generate_Patch), nameof(IsPocketMapWithCaves)));
            }
            else
            {
                yield return codes[i];
            }
        }
    }

}
