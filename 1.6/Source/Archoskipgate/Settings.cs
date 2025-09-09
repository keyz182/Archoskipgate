using UnityEngine;
using Verse;

namespace Archoskipgate;

public class Settings : ModSettings
{
    public int planetWidth = 275;
    public int planetHeight = 275;

    public void DoWindowContents(Rect wrect)
    {
        Listing_Standard options = new();
        options.Begin(wrect);

        options.Label("ASK_Settings_DefaultGatePlanetWidth".Translate(planetWidth));
        options.IntAdjuster(ref planetWidth, 10, 275);
        options.Gap();

        options.Label("ASK_Settings_DefaultGatePlanetHeight".Translate(planetHeight));
        options.IntAdjuster(ref planetHeight, 10, 275);
        options.Gap();

        options.End();
    }

    public override void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
        {
            if(planetHeight < 275)
            {
                planetHeight = 275;
            }
            if(planetWidth < 275)
            {
                planetWidth = 275;
            }
        }
        Scribe_Values.Look(ref planetWidth, "planetWidth", 275);
        Scribe_Values.Look(ref planetHeight, "planetHeight", 275);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if(planetHeight < 275)
            {
                planetHeight = 275;
            }
            if(planetWidth < 275)
            {
                planetWidth = 275;
            }
        }
    }
}
