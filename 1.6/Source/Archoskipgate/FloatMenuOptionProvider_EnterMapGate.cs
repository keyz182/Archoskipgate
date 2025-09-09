using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Archoskipgate;

public class FloatMenuOptionProvider_EnterMapGate: FloatMenuOptionProvider
{
  private static List<Pawn> tmpPortalEnteringPawns = new List<Pawn>();

  protected override bool Drafted => true;

  protected override bool Undrafted => true;

  protected override bool Multiselect => true;

  protected override bool MechanoidCanDo => true;

  public override IEnumerable<FloatMenuOption> GetOptionsFor(
      Thing clickedThing,
      FloatMenuContext context)
  {
      if (clickedThing is not Building_SkipGate portal || portal is Building_SkipGateExit)
          yield break;

      if (portal.IsOpen && (portal.planetMap == null || !portal.planetMap.mapPawns.FactionsOnMap().Contains(Faction.OfPlayer)))
      {
          yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(portal.CloseCommandString, delegate
          {
              Job job = JobMaker.MakeJob(ArchoskipgateDefOf.GDFP_CloseGate, portal);
              job.playerForced = true;
              context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, false);
          }, MenuOptionPriority.High, null, null, 0f, null, null, true, 0), context.FirstSelectedPawn, portal, "ReservedBy", null);
      }

        // TODO: Check if door is "opened" if not, option to open, otherwise, option to enter
        if (portal.IsOpen)
        {
            if (!portal.IsEnterable(out string reason))
            {
                yield return new FloatMenuOption("CannotEnterPortal".Translate(portal.Label) + ": " + reason.CapitalizeFirst(), null, MenuOptionPriority.Default,
                    null, null, 0f, null, null, true, 0);
            }
            else
            {
                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption("ASK_Enter".Translate(), delegate
                {
                    Job job = JobMaker.MakeJob(JobDefOf.EnterPortal, portal);
                    job.playerForced = true;
                    context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, false);
                }, MenuOptionPriority.High, null, null, 0f, null, null, true, 0), context.FirstSelectedPawn, portal, "ReservedBy", null);
            }
        }
        else
        {
            AcceptanceReport acceptanceReport = CanOpenPortal(context.FirstSelectedPawn, portal);
            if (!acceptanceReport.Accepted)
            {
                yield return new FloatMenuOption("GDFP_CannotOpen".Translate(portal.Label) + ": " + acceptanceReport.Reason.CapitalizeFirst(), null, MenuOptionPriority.Default,
                    null, null, 0f, null, null, true, 0);
            } else if (portal.selectedAddress == null)
            {
                yield return new FloatMenuOption("GDFP_NoAddressSelected".Translate(portal.Label), null, MenuOptionPriority.Default,
                    null, null, 0f, null, null, true, 0);
            }
            else
            {

                yield return FloatMenuUtility.DecoratePrioritizedTask(new FloatMenuOption(portal.OpenCommandString, delegate
                {
                    Job job = JobMaker.MakeJob(ArchoskipgateDefOf.GDFP_OpenGate, portal);
                    job.playerForced = true;
                    context.FirstSelectedPawn.jobs.TryTakeOrderedJob(job, JobTag.Misc, false);
                }, MenuOptionPriority.High, null, null, 0f, null, null, true, 0), context.FirstSelectedPawn, portal, "ReservedBy", null);
            }
        }
  }

  public static AcceptanceReport CanOpenPortal(Pawn pawn, MapPortal portal)
  {
      if (pawn == null)
      {
          return true;
      }

      if (!pawn.CanReach(portal, PathEndMode.ClosestTouch, Danger.Deadly, false, false, TraverseMode.ByPawn))
      {
          return "NoPath".Translate();
      }

      if (!pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation))
      {
          return "Incapable".Translate();
      }

      if (portal is Building_SkipGate { IsOpen: true })
      {
          return "Quacka'ai already open";
      }

      return true;
  }


  private static AcceptanceReport CanEnterPortal(Pawn pawn, MapPortal portal)
  {
    if (!pawn.CanReach((LocalTargetInfo) (Thing) portal, PathEndMode.ClosestTouch, Danger.Deadly))
      return "NoPath".Translate();
    return !pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation) ? "Incapable".Translate() : true;
  }
}
