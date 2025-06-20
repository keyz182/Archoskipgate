using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Archoskipgate;

public class JobGiver_GoThroughGate: ThinkNode_JobGiver
{
    public int radius;
    protected override Job TryGiveJob(Pawn pawn)
    {
        CompCanBeDormant compDormant = pawn.GetComp<CompCanBeDormant>();
        if (compDormant is null || compDormant.Awake)
        {
            List<Thing> gates = pawn.Map.listerThings.ThingsOfDef(ArchoskipgateDefOf.GDFP_QuakkaaiExit);

            if (gates.Any())
            {
                Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, gates, PathEndMode.Touch,
                    TraverseParms.For(TraverseMode.PassAllDestroyableThings),
                    radius, (t) => pawn.CanReserve(t));
                if (thing != null)
                {
                    return JobMaker.MakeJob(JobDefOf.EnterPortal, thing);
                }
            }
        }

        return null;
    }
}
