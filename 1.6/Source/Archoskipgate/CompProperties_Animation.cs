using Verse;

namespace Archoskipgate;

public class CompProperties_Animation : CompProperties
{
    public bool randomized = false;

    public int frameSpeed = 6;

    public GraphicData NextStep;

    public CompProperties_Animation()
    {
        compClass = typeof(CompAnimation);
    }
}
