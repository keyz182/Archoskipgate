using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KCSG;
using RimWorld;
using RimWorld.BaseGen;
using Verse;

namespace Archoskipgate;

public class GenStep_GDCustomStructureGen : GenStep_CustomStructureGen
{
    public IntVec2 mapSizeOverride;

    Lazy<FieldInfo> SLDSizes = new(()=> AccessTools.Field(typeof(StructureLayoutDef), "sizes"));

    public override void Generate(Map map, GenStepParams parms)
    {
        GenOption.customGenExt = new CustomGenOption
        {
            symbolResolvers = symbolResolvers, filthTypes = filthTypes, scatterThings = scatterThings, scatterChance = scatterChance,
        };

        // Tiled
        if (!tiledStructures.NullOrEmpty())
        {
            TileUtils.Generate(tiledStructures.RandomElement(), map.Center, map, scaleWithQuest ? CustomGenOption.GetRelatedQuest(map) : null);
            return;
        }

        List<CellRect> usedRects = new();
        if (structureLayoutDefs.Any())
        {
            foreach (StructureLayoutDef structureLayoutDef in structureLayoutDefs)
            {
                if (TryFindRect(out CellRect cellRect, ref usedRects, map, structureLayoutDef))
                {
                    GenOption.GetAllMineableIn(cellRect, map);
                    LayoutUtils.CleanRect(structureLayoutDef, map, cellRect, fullClear);
                    structureLayoutDef.Generate(cellRect, map);


                    if (GenOption.customGenExt.symbolResolvers?.Count > 0)
                    {
                        Debug.Message("GenStep_CustomStructureGen - Additional symbol resolvers");
                        BaseGen.symbolStack.Push("kcsg_runresolvers", new ResolveParams { faction = map.ParentFaction, rect = cellRect }, null);
                    }

                    PostGenerate(cellRect, map, parms);

                    StructureDefModExtension mde = structureLayoutDef.GetModExtension<StructureDefModExtension>();
                    if(mde.spawnedPawns.NullOrEmpty()) continue;
                    List<Pawn> pawns = map.mapPawns.AllPawns.ToList();
                    foreach (Pawn pawn in pawns)
                    {
                        pawn.Destroy();
                    }
                    foreach (PawnRepr mdeSpawnedPawn in mde.spawnedPawns)
                    {
                        mdeSpawnedPawn.SpawnPawn(map, Faction.OfAncientsHostile);
                    }
                }
                else
                {
                    ModLog.Warn("GenStep_CustomStructureGen - Could not find rect for " + structureLayoutDef.defName);
                }
            }
        }

        // Flood refog
        if (map.mapPawns.FreeColonistsSpawned.Count > 0)
        {
            FloodFillerFog.DebugRefogMap(map);
        }

        // Clear fog in rect if wanted
        if (clearFogInRect)
        {
            foreach (CellRect cellRect in usedRects)
            {
                foreach (IntVec3 c in cellRect)
                {
                    if (map.fogGrid.IsFogged(c))
                        map.fogGrid.Unfog(c);
                    else
                        MapGenerator.rootsToUnfog.Add(c);
                }
            }
        }
    }

    public bool TryFindRect(out CellRect cellRect, ref List<CellRect> usedRects, Map map, StructureLayoutDef structureLayoutDef)
    {
        int tries = 0;
        CellRect MapBounds = new(0,0, map.Size.x, map.Size.z);

        List<CellRect> localUsedRects = usedRects;

        while (tries++ < 200)
        {

            if (MapBounds.TryFindRandomInnerRect(((IntVec2) SLDSizes.Value.GetValue(structureLayoutDef)), out cellRect,
                    (rect) => !localUsedRects.Any(ur => ur.Overlaps(rect.ExpandedBy(5)))))
            {
                usedRects.Add(cellRect);
                return true;
            }
        }

        cellRect = default;
        return false;
    }
}
