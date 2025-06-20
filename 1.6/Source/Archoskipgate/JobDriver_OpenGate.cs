using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace Archoskipgate;

public class JobDriver_OpenGate : JobDriver
{
    public Building_SkipGate Gate => TargetThingA as Building_SkipGate;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return true;
    }

    public override string GetReport()
    {
        string reportStringOverride = base.GetReport();

        if (Gate.IsOpen)
        {
            return "GDFP_ReportString_GateOpened".Translate();
        }

        if (Gate.selectedAddress == null)
        {
            return "GDFP_ReportString_GateNotSelected".Translate();
        }

        return reportStringOverride;
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedOrNull(TargetIndex.A);
        this.FailOn(() => Gate.IsOpen);
        this.FailOn(() => Gate.selectedAddress == null);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch, false);

        Toil toilFaceAndWait = Toils_General.Wait(90, TargetIndex.None).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch).WithProgressBarToilDelay(TargetIndex.A, true, -0.5f);

        toilFaceAndWait.tickAction = (Action) Delegate.Combine(toilFaceAndWait.tickAction, new Action(delegate
        {
            pawn.rotationTracker.FaceTarget(job.targetA);
        }));
        toilFaceAndWait.handlingFacing = true;

        yield return toilFaceAndWait;

        Toil workToOpenGate = Toils_General.Wait(5, TargetIndex.None)
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.ClosestTouch);

        if(Gate.IsMainGate)
            workToOpenGate.AddPreInitAction((() =>
            {
                LongEventHandler.QueueLongEvent(() =>
                {
                    Gate.GenerateNewPlanetMap();
                }, "GDFP_OpeningPortal", true, null);;
            }));

        workToOpenGate.AddFinishAction(delegate
        {
            Gate.OpenGate();

        });
        yield return workToOpenGate;
    }
}
