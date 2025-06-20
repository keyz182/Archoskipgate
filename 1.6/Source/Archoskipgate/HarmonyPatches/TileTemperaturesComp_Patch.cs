using System;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Archoskipgate.HarmonyPatches;

[HarmonyPatch(typeof(TileTemperaturesComp))]
public static class TileTemperaturesComp_Patch
{
    public static Type TemperatureCacheType = AccessTools.TypeByName("CachedTileTemperatureData");

    [HarmonyPatch("RetrieveCachedData")]
    [HarmonyPrefix]
    public static void RetrieveCachedData_Patch(ref int tile)
    {
        if (tile < 0 || tile > Find.WorldGrid.TilesCount)
        {
            if(Find.CurrentMap.Tile > 0)
                tile = Find.CurrentMap.Tile;
            else
                tile = Rand.Range(0, Find.WorldGrid.TilesCount);
        }
    }
}
