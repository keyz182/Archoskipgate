using UnityEngine;
using Verse;

namespace Archoskipgate;

public class Graphic_Animated: Graphic_Indexed
{
    private int currentFrame = 0;

    private int ticksPerFrame = 6;

    private int ticksPrev = 0;

    private GraphicData NextStep;

    public override Material MatSingle
    {
        get
        {
            return subGraphics[currentFrame].MatSingle;
        }
    }


    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        if(thingDef == null)
        {
            Log.Error("Graphic_Animated with null thingDef");
            return;
        }
        if(subGraphics == null)
        {
            Log.Error("Graphic_Animated has no subgraphics");
            return;
        }

        if (!thingDef.HasComp(typeof(CompAnimation)))
        {
            Log.Error("Graphic_Animated has no CompAnimation");
            return;
        }

        CompAnimation comp = thing.TryGetComp<CompAnimation>();
        ticksPerFrame = comp.Props.frameSpeed;

        if (this != comp.Props.NextStep.Graphic && comp.Reset)
        {
            NextStep = null;
            comp.Reset = false;
        }


        int ticksCurrent = Find.TickManager.TicksGame;
        if(ticksCurrent >= ticksPrev + ticksPerFrame)
        {
            ticksPrev = ticksCurrent;
            currentFrame++;
        }

        if(currentFrame >= subGraphics.Length)
        {
            currentFrame = 0;
            NextStep ??= comp.Props.NextStep;
        }

        if (this != comp.Props.NextStep.Graphic && NextStep != null)
        {
            NextStep.Graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
            return;
        }

        Graphic graphic = subGraphics[currentFrame];
        graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);

        ShadowGraphic?.DrawWorker(loc, rot, thingDef, thing, extraRotation);

    }
}
