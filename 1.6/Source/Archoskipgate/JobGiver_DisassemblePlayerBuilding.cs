using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Archoskipgate;

public class JobGiver_DisassemblePlayerBuilding : ThinkNode_JobGiver
{
    public int radius;
    protected override Job TryGiveJob(Pawn pawn)
    {
        CompCanBeDormant compDormant = pawn.GetComp<CompCanBeDormant>();
        if (compDormant is null || compDormant.Awake)
        {
            List<Thing> allThings = pawn.Map.listerThings.AllThings.Where(x => x.Faction == Faction.OfPlayer && x.def.building != null && x.def != ArchoskipgateDefOf.GDFP_QuakkaaiExit && x.def != ArchoskipgateDefOf.GDFP_Quakkaai).ToList();

            if (allThings.Any())
            {
                Thing thing = GenClosest.ClosestThing_Global_Reachable(pawn.Position, pawn.Map, allThings, PathEndMode.Touch,
                    TraverseParms.For(TraverseMode.PassAllDestroyableThings),
                    radius, (t) => pawn.CanReserve(t));
                if (thing != null)
                {
                    return JobMaker.MakeJob(ArchoskipgateDefOf.GDFP_Replicate, thing);
                }
            }
        }

        return null;
    }

    public static bool HasDutyAndShouldStayInGroup(Pawn pawn)
    {
        if (pawn.mindState.duty == null || !pawn.mindState.duty.focus.IsValid) return false;

        if (pawn.GetLord()?.LordJob is not LordJob_SleepThenMechanoidsDefend) return false;

        JobDef firstJobInGroup = pawn.GetLord().ownedPawns?.FirstOrDefault(x => x is { Spawned: true, Dead: false } && x.def != pawn.def)?.CurJobDef;
        return firstJobInGroup != null && (firstJobInGroup == JobDefOf.Wait_Wander || firstJobInGroup == JobDefOf.GotoWander);
    }
}
