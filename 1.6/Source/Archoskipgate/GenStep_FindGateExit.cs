using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Archoskipgate;

public class GenStep_FindGateExit : GenStep
{
    public const float ClearRadius = 4.5f;

    public override int SeedPart => 12412314;

    public override void Generate(Map map, GenStepParams parms)
    {
        Building bld = map.listerBuildings.AllBuildingsNonColonistOfDef(ArchoskipgateDefOf.GDFP_QuakkaaiExit).FirstOrDefault();
        if (bld != null)
        {
            MapGenerator.PlayerStartSpot = bld.InteractionCell;
            return;
        }

        if (!RCellFinder.TryFindRandomCellNearWith(map.Center, (vec3) => ValidateCell(vec3, map), map, out IntVec3 result))
        {
            CellFinder.TryFindRandomCell(map, cell => cell.Standable(map) && cell.DistanceToEdge(map) > 5.5, out result);
            foreach (IntVec3 c in GenRadial.RadialCellsAround(result, 6f, true))
            {
                foreach (Thing thing in c.GetThingList(map).ToList().Where(t => t.def.destroyable))
                    thing.Destroy();
            }
        }

        GenSpawn.Spawn(ThingMaker.MakeThing(ArchoskipgateDefOf.GDFP_QuakkaaiExit), result, map);
        MapGenerator.PlayerStartSpot = result;
    }

    public bool ValidateCell(IntVec3 c, Map map)
    {
        IEnumerable<IntVec3> cells = GenRadial.RadialCellsAround(c, 6f, true);
        return map.thingGrid.ThingsListAt(c).Count == 0 && cells.Count(cell => cell.Standable(map)) > 0;
    }
}
