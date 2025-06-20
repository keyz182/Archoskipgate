using Verse;
using UnityEngine;
using HarmonyLib;

namespace Archoskipgate;

public class ArchoskipgateMod : Mod
{
    public static Settings settings;

    public ArchoskipgateMod(ModContentPack content) : base(content)
    {
        Log.Message("Hello world from Archoskipgate");

        // initialize settings
        settings = GetSettings<Settings>();
#if DEBUG
        Harmony.DEBUG = true;
#endif
        Harmony harmony = new Harmony("keyz182.rimworld.Archoskipgate.main");	
        harmony.PatchAll();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        settings.DoWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return "Archoskipgate_SettingsCategory".Translate();
    }
}
