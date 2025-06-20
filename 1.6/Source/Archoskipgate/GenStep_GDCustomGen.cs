using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using KCSG;
using RimWorld;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;

namespace Archoskipgate;

public class GenStep_GDCustomGen : GenStep_CustomStructureGen
{
    public static void AuthorLetter(Map map, List<string> authors)
    {
        TaggedString label = "GDFP_Author_Title".Translate();

        string authorStr = string.Join("\n", authors.Distinct().InRandomOrder());

        TaggedString text = "GDFP_Author_More".Translate(authorStr);

        AuthorLetter authorLetter = (AuthorLetter) LetterMaker.MakeLetter(ArchoskipgateDefOf.GDFP_MapAuthor);
        authorLetter.Label = label;
        authorLetter.Text = text;

        Find.LetterStack.ReceiveLetter(authorLetter);
    }

    public override int SeedPart => 1241521352;


    Lazy<FieldInfo> SLDSizes = new(()=> AccessTools.Field(typeof(StructureLayoutDef), "sizes"));

    public override void Generate(Map map, GenStepParams parms)
    {
        List<CellRect> usedRects;
        if (!MapGenerator.TryGetVar("UsedRects", out usedRects))
        {
            usedRects = new List<CellRect>();
            MapGenerator.SetVar("UsedRects", usedRects);
        }

        GenOption.customGenExt = new CustomGenOption
        {
            symbolResolvers = symbolResolvers, filthTypes = filthTypes, scatterThings = scatterThings, scatterChance = scatterChance,
        };

        if (GateAddress.CurrentGateAddress.structureLayouts.Any())
        {
            List<string> authors = [];
            foreach (StructureLayoutDef structureLayoutDef in GateAddress.CurrentGateAddress.structureLayouts)
            {
                string author = structureLayoutDef.GetModExtension<StructureDefModExtension>().author;
                authors.Add(author);

                LongEventHandler.SetCurrentEventText("GDFP_LoadingBy".Translate(author));

                if (TryFindRect(out CellRect cellRect, ref usedRects, map, structureLayoutDef))
                {
                    GenOption.GetAllMineableIn(cellRect, map);
                    LayoutUtils.CleanRect(structureLayoutDef, map, cellRect, true);
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
                    Faction faction = GateAddress.SelectedFaction ?? (mde.anyHostile ? Faction.OfAncientsHostile : Faction.OfAncients);

                    LordJob_DefendBase lordJobDefendBase = new(faction, mde.lordCenter.IsValid ? mde.lordCenter : map.Center, true);
                    Lord lord = LordMaker.MakeNewLord(faction, lordJobDefendBase, map);

                    foreach (PawnRepr mdeSpawnedPawn in mde.spawnedPawns)
                    {
                        mdeSpawnedPawn.SpawnPawn(map, faction, lord);
                    }
                }
                else
                {
                    ModLog.Warn("GenStep_CustomStructureGen - Could not find rect for " + structureLayoutDef.defName);
                }
            }


            if (authors.NullOrEmpty()) return;

            AuthorLetter(map, authors);
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
