using System;
using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using Verse;
using Verse.Sound;
using StructureLayoutDef = KCSG.StructureLayoutDef;

namespace Archoskipgate;

public class Building_SkipGate: MapPortal
{
    public static GDFP_WorldComponent WorldComponent => Find.World.GetComponent<GDFP_WorldComponent>();

    // Handle alt texture for door closed
    private GDFPModExtension defExtension;

    public Graphic gateOpeningGraphic;
    protected bool HasExtension => defExtension != null;
    public virtual bool IsMainGate => true;

    public GateAddress selectedAddress;


    public bool isOpen = false;

    public virtual bool IsOpen
    {
        get => isOpen;
        set=>isOpen = value;
    }

    public void OpenGate()
    {
        if(selectedAddress == null) return;

        IsOpen = true;
        if (Find.CurrentMap == Map)
        {
            ArchoskipgateDefOf.GDFP_Activate.PlayOneShot((SoundInfo) (Thing) this);
        }
        else
        {
            if (Find.CurrentMap != exitGate.Map)
                return;
            ArchoskipgateDefOf.GDFP_Activate.PlayOneShot((SoundInfo) (Thing) exitGate);
        }
    }

    public virtual void CloseGate()
    {
        IsOpen = false;
        exitGate = null;
        selectedAddress = null;

        PocketMapUtility.DestroyPocketMap(planetMap);
        planetMap = null;
    }


    public override Graphic Graphic => IsOpen ? AlternateGraphic : base.Graphic;

    protected Graphic AlternateGraphic
    {
        get
        {
            if (gateOpeningGraphic != null)
            {
                return gateOpeningGraphic;
            }

            if (!HasExtension || defExtension.openingGraphicData == null)
            {
                return BaseContent.BadGraphic;
            }

            gateOpeningGraphic = defExtension.openingGraphicData.GraphicColoredFor(this);

            return gateOpeningGraphic;
        }
    }


    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        defExtension = def.GetModExtension<GDFPModExtension>();
    }

    public virtual string OpenCommandString => "GDFP_OpenPortal".Translate(Label);
    public virtual string CloseCommandString => "GDFP_ClosePortal".Translate(Label);


    public Map planetMap;
    public Building_SkipGateExit exitGate;

    public override Map GetOtherMap()
    {
        if (planetMap == null)
            GenerateNewPlanetMap();
        return planetMap;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref isOpen, "isOpen");
        Scribe_References.Look(ref planetMap, "planetMap");
        Scribe_References.Look(ref exitGate, "exitGate");
        Scribe_References.Look(ref selectedAddress, "selectedAddress");
    }

    public override IntVec3 GetDestinationLocation()
    {
        return exitGate?.Position ?? IntVec3.Invalid;
    }

    public override void OnEntered(Pawn pawn)
    {
        base.OnEntered(pawn);

        selectedAddress.Visited = true;
        if (Find.CurrentMap == Map)
        {
            ArchoskipgateDefOf.GDFP_Travel.PlayOneShot((SoundInfo) (Thing) this);
        }
        else
        {
            if (Find.CurrentMap != exitGate.Map)
                return;
            ArchoskipgateDefOf.GDFP_Travel.PlayOneShot((SoundInfo) (Thing) exitGate);
        }
    }

    public virtual void GenerateNewPlanetMap()
    {
        if(planetMap != null) return;

        GateAddress.GenerateNewPlanetMap_V2(this, out planetMap, out exitGate, selectedAddress);
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo gizmo in base.GetGizmos())
            yield return gizmo;

        if (DebugSettings.ShowDevGizmos)
        {
            Command_Action newRandAddress = new Command_Action();
            newRandAddress.defaultLabel = "GDFP_GenNewRandAddress".Translate();
            newRandAddress.action = () =>
            {
                GateAddress address = new()
                {
                    address = GateAddress.RandomGateAddressString(),
                    biome = GateAddress.GetBiome(),
                    name = GateAddress.RandomGateName(),
                    structureLayouts = GateAddress.GetRandomStructureLayouts(),
                };

                address.description = $"Biome = {address.biome.label}; Avg Temperature: {address.temperature}";

                WorldComponent.LearnedAddresses.Add(address);
                Messages.Message("GDFP_LearnedAddress".Translate(address.address), MessageTypeDefOf.PositiveEvent);
            };
            yield return (Gizmo) newRandAddress;


            Command_Action genNewAddress = new Command_Action();
            genNewAddress.defaultLabel = "GDFP_GenNewAddress".Translate();
            genNewAddress.action = () =>
            {
                List<FloatMenuOption> floatMenuOptionList = new List<FloatMenuOption>();
                List<StructureLayoutDef> layouts = DefDatabase<StructureLayoutDef>.AllDefsListForReading.Where(sld => sld.HasModExtension<StructureDefModExtension>()).Where(sld=>!sld.GetModExtension<StructureDefModExtension>().excludeFromRandomGen).ToList();

                foreach (StructureLayoutDef layout in layouts)
                {
                    floatMenuOptionList.Add(new FloatMenuOption(layout.defName, (Action) (() =>
                    {
                        GateAddress address = new()
                        {
                            address = GateAddress.RandomGateAddressString(),
                            biome = GateAddress.GetBiome(),
                            name = GateAddress.RandomGateName(),
                            structureLayouts = [layout],
                        };

                        address.description = $"Biome = {address.biome.label}; Avg Temperature: {address.temperature}";

                        WorldComponent.LearnedAddresses.Add(address);
                        Messages.Message("GDFP_LearnedAddress".Translate(address.address), MessageTypeDefOf.PositiveEvent);
                    })));
                    if (!floatMenuOptionList.Any())
                        return;
                    Find.WindowStack.Add(new FloatMenu(floatMenuOptionList));
                }

            };
            yield return (Gizmo) genNewAddress;
        }
    }

}
