using RimWorld;
using Verse;

namespace Archoskipgate;

[DefOf]
public static class ArchoskipgateDefOf
{
    // Remember to annotate any Defs that require a DLC as needed e.g.
    // [MayRequireBiotech]
    // public static GeneDef YourPrefix_YourGeneDefName;
    
    static ArchoskipgateDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ArchoskipgateDefOf));
}
