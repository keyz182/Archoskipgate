using RimWorld;
using Verse;
using Verse.Sound;

namespace Archoskipgate;

public class Building_SkipGateExit: Building_SkipGate
{
    public override bool IsMainGate => false;

    public Building_SkipGate entryGate;

    public override bool IsOpen
    {
        get => entryGate != null;
    }

    public override void CloseGate()
    {
        return;
    }


    public override Map GetOtherMap() => entryGate.Map;

    public override IntVec3 GetDestinationLocation() => entryGate.Position;
    public override void OnEntered(Pawn pawn)
    {
        base.OnEntered(pawn);
        if (Find.CurrentMap == Map)
        {
            SoundDefOf.TraversePitGate.PlayOneShot((SoundInfo) (Thing) this);
        }
        else
        {
            if (Find.CurrentMap != entryGate.Map)
                return;
            SoundDefOf.TraversePitGate.PlayOneShot((SoundInfo) (Thing) entryGate);
        }
    }
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref entryGate, "entryGate");
    }

}
