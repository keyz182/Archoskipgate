using System.Collections.Generic;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Archoskipgate;

public class StockGenerator_DGC : StockGenerator
{
    public float chance;

    public GDFP_WorldComponent worldComponent => Find.World.GetComponent<GDFP_WorldComponent>();

    public override IEnumerable<Thing> GenerateThings(PlanetTile forTile, Faction faction = null)
    {
        if (worldComponent.LearnedAddresses.Any(ga => ga.name == "DGC")) yield break;

        if(Rand.Value < chance) yield break;

        Thing book = ThingMaker.MakeThing(ArchoskipgateDefOf.GDFP_GateAddressBookSGC);
        book.stackCount = 1;
        yield return book;
    }

    public override bool HandlesThingDef(ThingDef thingDef)
    {
        return thingDef == ArchoskipgateDefOf.GDFP_GateAddressBookSGC;
    }

}
