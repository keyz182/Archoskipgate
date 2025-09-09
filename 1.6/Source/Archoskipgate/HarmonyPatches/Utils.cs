using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace Archoskipgate.HarmonyPatches;

public static class Utils
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

}
