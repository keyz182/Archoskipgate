using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using Verse;

namespace Archoskipgate;

public class GenStep_Fog : GenStep
{
    public static Lazy<MethodInfo> SetAllFogged = new Lazy<MethodInfo>(()=>AccessTools.Method(typeof(FogGrid), "SetAllFogged"));
    public override int SeedPart => 1568957891;

    public override void Generate(Map map, GenStepParams parms)
    {
        DeepProfiler.Start("GenerateInitialFogGrid");
        SetAllFogged.Value.Invoke(map.fogGrid, null);

        if (MapGenerator.PlayerStartSpot != IntVec3.Zero)
            FloodFillerFog.FloodUnfog(MapGenerator.PlayerStartSpot, map);

        List<IntVec3> rootsToUnfog = MapGenerator.rootsToUnfog;
        foreach (IntVec3 t in rootsToUnfog)
            FloodFillerFog.FloodUnfog(t, map);

        DeepProfiler.End();
    }
}
