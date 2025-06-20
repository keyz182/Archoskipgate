using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Archoskipgate;

public class StructureDefModExtension: DefModExtension
{
    public string author;
    public bool standalone = true;
    public bool doLoot = true;
    public bool anyHostile = false;
    public bool excludeFromRandomGen = false;
    public BiomeDef biome;
    public IntVec2 size;
    public IntVec3 lordCenter = IntVec3.Invalid;
    public FactionDef pawnFaction;
    public string pawnFactionSearchString;
    public List<PawnRepr> spawnedPawns;
    public List<GenStepDef> extraGenSteps;
}
