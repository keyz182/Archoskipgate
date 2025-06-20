using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using Verse.AI.Group;

namespace Archoskipgate;

public class JobDriver_Replicate : JobDriver
{
    public IntRange replicatesRange = new(0, 2);
    protected Thing Target => job.targetA.Thing;
    protected Building Building => (Building)Target.GetInnerIfMinified();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOn(() => JobGiver_DisassemblePlayerBuilding.HasDutyAndShouldStayInGroup(pawn));
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        Toil doWork = new Toil().FailOnDestroyedOrNull(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
        doWork.tickAction = delegate
        {

            if (doWork.actor.IsHashIntervalTick(30)) return;

            Target.HitPoints -= 1;
            if (Target.HitPoints <= 0f)
            {
                doWork.actor.jobs.curDriver.ReadyForNextToil();
            }
        };
        doWork.defaultCompleteMode = ToilCompleteMode.Never;
        doWork.WithProgressBar(TargetIndex.A, () => Target.HitPoints / (float)Target.MaxHitPoints);
        yield return doWork;
        Toil toil = new() { initAction = delegate
            {
                FinishedRemoving();
                Map.designationManager.RemoveAllDesignationsOn(Target);
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return toil;
    }

    protected void FinishedRemoving()
    {
        Target.Destroy(DestroyMode.Refund);
        if (Find.PlaySettings.autoRebuild)
        {
            GenConstruct.PlaceBlueprintForBuild(Target.def, Target.Position, Map, Target.Rotation, Faction.OfPlayer, Target.Stuff);
        }

        int num = replicatesRange.RandomInRange;
        for (int i = 0; i < num; i++)
        {
            Pawn replicated = PawnGenerator.GeneratePawn(pawn.kindDef, pawn.Faction);
            GenSpawn.Spawn(replicated, pawn.Position, Map);
            if (pawn.TryGetLord(out Lord lord))
            {
                lord.AddPawn(replicated);
            }
        }
    }
}
