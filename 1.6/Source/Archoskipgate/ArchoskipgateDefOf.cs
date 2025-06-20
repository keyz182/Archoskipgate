using RimWorld;
using Verse;

namespace Archoskipgate;

[DefOf]
public static class ArchoskipgateDefOf
{
    public static SoundDef GDFP_Travel;
    public static SoundDef GDFP_Activate;

    public static JobDef GDFP_OpenGate;
    public static JobDef GDFP_CloseGate;
    public static JobDef GDFP_Replicate;

    public static ThingDef GDFP_Quakkaai;
    public static ThingDef GDFP_QuakkaaiExit;
    public static ThingDef GDFP_StrangeLetter;
    public static ThingDef GDFP_GateAddressBookSGC;

    public static MapGeneratorDef GDFP_Planet;
    public static MapGeneratorDef GDFP_PlanetStandalone;

    public static LetterDef GDFP_MapAuthor;

    static ArchoskipgateDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(ArchoskipgateDefOf));
}
