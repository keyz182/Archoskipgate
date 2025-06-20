using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;

namespace Archoskipgate.HarmonyPatches;

[HarmonyPatch(typeof(GenStep_Settlement))]
[HarmonyPatch("ScatterAt")]
public static class GenStep_Settlement_Patch
{
    [HarmonyPatch("ScatterAt")]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        foreach (CodeInstruction instruction in instructions)
        {
            if (instruction.opcode == OpCodes.Stfld && instruction.operand is FieldInfo { Name: "faction" } field && field.DeclaringType == typeof(RimWorld.BaseGen.ResolveParams))
            {
                // Before storing the faction, call our GetFaction method
                yield return new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(GenStep_Settlement_Patch), nameof(GetFaction)));
            }
            yield return instruction;
        }
    }


    private static Faction GetFaction(Faction originalFaction)
    {
        return GateAddress.SelectedFaction ?? originalFaction;
    }
}
