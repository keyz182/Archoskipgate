using Verse;

namespace Archoskipgate;

public class GenStep_Pollution : GenStep
{
    public static readonly FloatRange LightPollutionCellRange = new FloatRange(0.1f, 0.3f);
    public static readonly FloatRange ModeratePollutionCellRange = new FloatRange(0.4f, 0.6f);
    public const int PollutionGlobSize = 1000;

    public override int SeedPart => 7594115;

    public override void Generate(Map map, GenStepParams parms)
    {
        PollutionUtility.PolluteMapToPercent(map, map.TileInfo.pollution);
    }
}
