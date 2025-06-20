using System;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Archoskipgate.HarmonyPatches;

[HarmonyPatch(typeof(WildAnimalSpawner))]
public static class WildAnimalSpawner_CommonalityOfAnimalNow_Transpiler
{
    public static Lazy<FieldInfo> MapField = new(() => AccessTools.Field(typeof(WildAnimalSpawner), "map"));

    [HarmonyPatch("CommonalityOfAnimalNow")]
    [HarmonyPrefix]
    public static bool CommonalityOfAnimalNow(WildAnimalSpawner __instance, ref float __result,  PawnKindDef def)
    {
        Map map = MapField.Value.GetValue(__instance) as Map;

        if (map == null) return true;

        __result = (!ModsConfig.BiotechActive || Rand.Value >= (double) WildAnimalSpawner.PollutionAnimalSpawnChanceFromPollutionCurve.Evaluate(map.TileInfo.pollution) ? map.Biome.CommonalityOfAnimal(def) : map.Biome.CommonalityOfPollutionAnimal(def)) / def.wildGroupSize.Average;
        return false;
    }
}


