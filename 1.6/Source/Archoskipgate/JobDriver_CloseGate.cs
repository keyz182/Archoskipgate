using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Archoskipgate;

public class JobDriver_CloseGate : JobDriver
{
    public Building_SkipGate Gate => TargetThingA as Building_SkipGate;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override string GetReport()
    {
        string reportStringOverride = base.GetReport();

        if (!Gate.IsOpen)
        {
            return "GDFP_ReportString_GateNotOpened".Translate();
        }

        if (Gate.planetMap.mapPawns.FactionsOnMap().Contains(Faction.OfPlayer))
        {
            return "GDFP_ReportString_PlayerStillOnMap".Translate();
        }

        return reportStringOverride;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => !Gate.IsOpen);
        this.FailOn(() => Gate.planetMap.mapPawns.FactionsOnMap().Contains(Faction.OfPlayer));

        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, false);

        Toil toilFaceAndWait = Toils_General.Wait(90, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, true, -0.5f);

        toilFaceAndWait.tickAction = (Action) Delegate.Combine(toilFaceAndWait.tickAction, new Action(delegate
        {
            pawn.rotationTracker.FaceTarget(job.targetA);
        }));
        toilFaceAndWait.handlingFacing = true;
        yield return toilFaceAndWait;

        Toil workToCloseGate = Toils_General.Wait(300, TargetIndex.None)
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch)
            .WithProgressBarToilDelay(TargetIndex.A, 300, false, -0.5f);

        workToCloseGate.AddFinishAction(delegate
        {
            ModLog.Log($"Ensuring Map from Job {this}");
            Gate.CloseGate();

        });
        yield return workToCloseGate;
    }
}
