using System;
using System.Collections.Generic;
using System.Linq;
using KCSG;
using RimWorld;
using RimWorld.Planet;
using Verse;
using StructureLayoutDef = KCSG.StructureLayoutDef;

namespace Archoskipgate;

public class GateAddress: IExposable, ILoadReferenceable
{
    public static GateAddress CurrentGateAddress;
    public static Faction SelectedFaction;
    public static GDFP_WorldComponent WorldComponent => Find.World.GetComponent<GDFP_WorldComponent>();
    public static readonly char[] GateAddressSymbols = "öøÿĂƋƍƎƔƛƱƼǂƿƾȣȸɁɄɣɞɸɷʘʭʢʞ".ToCharArray();

    public bool Visited = false;

    public string address;
    public BiomeDef biome;
    public float temperature;
    public int layer;
    public int tile;
    public string name;
    public string description;
    public IncidentDef incidentToTrigger;
    public FactionDef faction;
    public Map mapReference;

    public List<StructureLayoutDef> structureLayouts;
    public List<GenStepDef> extraGenSteps;
    public List<GenStepDef> chosenStructures;

    public Map Map => mapReference;
    public static string RandomGateAddressString()
    {
        return new string(Enumerable.Repeat(GateAddressSymbols, 7)
            .Select(s => s.RandomElement()).ToArray());
    }

    public static GateAddress RandomGateAddress()
    {
        GateAddress address = new() { address = RandomGateAddressString(), biome = GetBiome() };
        PlanetLayer layer = GetLayer();
        Tile biomeTile = GetWorldTileMatching(address.biome, layer);
        address.layer = layer.LayerID;
        address.tile = biomeTile.Layer.Tiles.IndexOf(biomeTile);
        address.temperature = biomeTile.temperature;
        return address;
    }

    public static string RandomGateName()
    {
        return $"P{Rand.Range(1, 9)}{Rand.Element(["A", "P", "Z", "X"])}-{Rand.Range(100, 999)}";
    }

    public static BiomeDef GetBiome()
    {
        return DefDatabase<BiomeDef>.AllDefs.Where(b => b.generatesNaturally).RandomElement();
    }

    public static PlanetLayer GetLayer()
    {
        return Find.World.grid.PlanetLayers.Values.RandomElement();
    }

    public static Tile GetWorldTileMatching(float temperature, PlanetLayer layer)
    {
        // expand range up to +/- 20.5 as needed to find the closes match, otherwise fall back to random
        for (int i = 0; i < 20; i++)
        {
            float tempRange = temperature + (0.5f + i);

            Tile tile =  layer.Tiles.Where(t => !t.WaterCovered && t.temperature > (temperature - tempRange) && t.temperature < (temperature + tempRange)).RandomElement();
            if (tile != null) return tile;
        }
        return layer.Tiles.Where(t => !t.WaterCovered).RandomElement();
    }

    public static Tile GetWorldTileMatching(BiomeDef biome, PlanetLayer layer)
    {
        Tile tile =  layer.Tiles.Where(t => !t.WaterCovered && t.PrimaryBiome == biome).RandomElement();
        return tile ?? layer.Tiles.Where(t => !t.WaterCovered).RandomElement();
    }

    public string Description
    {
        get {
            if (!string.IsNullOrEmpty(description)) return description;
            return "GDFP_AddressInspectorDescription".Translate(biome.label, temperature);
        }
    }

    public string Name => !string.IsNullOrEmpty(name) ? name : address;

    public PlanetLayer Layer => Find.World.grid.PlanetLayers.Values.First(p => p.LayerID == layer);

    public void ExposeData()
    {
        Scribe_Values.Look(ref address, "address");
        Scribe_Defs.Look(ref biome, "biome");
        Scribe_Values.Look(ref temperature, "temperature");
        Scribe_Values.Look(ref tile, "tile");
        Scribe_Values.Look(ref name, "name");
        Scribe_Values.Look(ref description, "description");
        // Scribe_References.Look(ref map, "map");
        Scribe_References.Look(ref mapReference, "mapReference");
        Scribe_Defs.Look(ref incidentToTrigger, "incidentToTrigger");
        Scribe_Defs.Look(ref faction, "faction");
        Scribe_Collections.Look(ref extraGenSteps, "extraGenSteps", LookMode.Def);
        Scribe_Collections.Look(ref chosenStructures, "chosenStructures", LookMode.Def);
        Scribe_Collections.Look(ref structureLayouts, "structureLayouts", LookMode.Def);
    }

    public static void GenerateNewPlanetMap(Building_SkipGate entryGate, out Map planetMap, out Building_SkipGateExit exitGate, GateAddress address = null)
    {
        address ??= RandomGateAddress();

        CurrentGateAddress = address;

        List<GenStepWithParams> gtwp = address.extraGenSteps == null ? [] : address.extraGenSteps.Select(gs => new GenStepWithParams(gs, new GenStepParams())).ToList();

        PocketMapParent mapParent = WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PocketMap) as PocketMapParent;
        mapParent!.sourceMap = entryGate.Map;
        planetMap = MapGenerator.GenerateMap(new IntVec3(ArchoskipgateMod.settings.planetWidth, 1, ArchoskipgateMod.settings.planetHeight), mapParent, ArchoskipgateDefOf.GDFP_Planet, gtwp, isPocketMap: true, extraInitBeforeContentGen: (map) =>
        {
            if (address.faction != null && Find.FactionManager.FirstFactionOfDef(address.faction) != null)
            {
                SelectedFaction = Find.FactionManager.FirstFactionOfDef(address.faction);
            }else if (gtwp.Any(s => s.def?.defName?.ToLower().Contains("settlement") ?? false))
            {
                SelectedFaction = Find.FactionManager.RandomNonHostileFaction();
            }
            map.TileInfo.PrimaryBiome = address.biome;
            map.TileInfo.temperature = address.temperature;
        });
        Find.World.pocketMaps.Add(mapParent);

        exitGate = planetMap.listerThings.ThingsOfDef(ArchoskipgateDefOf.GDFP_QuakkaaiExit).First() as Building_SkipGateExit;
        if(exitGate != null)
            exitGate.entryGate = entryGate;

        WorldComponent.AddNewAddressAndGate(CurrentGateAddress, planetMap);

        if (address.incidentToTrigger != null)
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(address.incidentToTrigger.category, planetMap);
            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(address.incidentToTrigger, null, parms), Find.TickManager.TicksGame + 300));
        }

        CurrentGateAddress = null;
        SelectedFaction = null;
    }


    public static void GenerateNewPlanetMap_V2(Building_SkipGate entryGate, out Map planetMap, out Building_SkipGateExit exitGate, GateAddress address = null)
    {
        address ??= RandomGateAddress();

        CurrentGateAddress = address;

        List<StructureLayoutDef> layouts = address.structureLayouts == null ? [] : address.structureLayouts.ToList();
        if (layouts.NullOrEmpty())
        {
            GenerateNewPlanetMap(entryGate, out planetMap, out exitGate, address);
            return;
        }

        IntVec3 mapSize = new IntVec3(ArchoskipgateMod.settings.planetWidth, 1, ArchoskipgateMod.settings.planetHeight);

        StructureLayoutDef standalone = null;
        List<GenStepWithParams> gtwp = [];
        MapGeneratorDef mapGenDef = ArchoskipgateDefOf.GDFP_Planet;

        foreach (StructureLayoutDef layout in layouts)
        {
            if (!layout.HasModExtension<StructureDefModExtension>()) continue;

            StructureDefModExtension sde = layout.GetModExtension<StructureDefModExtension>();

            if (standalone == null && sde.standalone)
            {
                standalone = layout;
                mapGenDef = ArchoskipgateDefOf.GDFP_PlanetStandalone;
                mapSize = new IntVec3(standalone.Sizes.x, 1, standalone.Sizes.z);
            }

            if (!sde.extraGenSteps.NullOrEmpty())
            {
                gtwp.AddRange(sde.extraGenSteps.Select(gs => new GenStepWithParams(gs, new GenStepParams())));
            }
        }

        gtwp.AddRange(address.extraGenSteps == null ? [] : address.extraGenSteps.Select(gs => new GenStepWithParams(gs, new GenStepParams())));

        if (WorldObjectMaker.MakeWorldObject(WorldObjectDefOf.PocketMap) is not PocketMapParent mapParent)
        {
            exitGate = null;
            planetMap = null;
            return;
        }
        mapParent.sourceMap = entryGate.Map;

        planetMap = MapGenerator.GenerateMap(mapSize, mapParent, mapGenDef, gtwp, isPocketMap: true, extraInitBeforeContentGen: map =>
        {
            if (standalone != null)
            {
                StructureDefModExtension sde = standalone.GetModExtension<StructureDefModExtension>();

                if (!string.IsNullOrEmpty(sde.pawnFactionSearchString))
                {
                    SelectedFaction = Find.FactionManager.AllFactions.FirstOrDefault(fac => fac.Name.ToLower().Contains(sde.pawnFactionSearchString.ToLower()));
                    if (SelectedFaction == null)
                    {
                        SelectedFaction = Find.FactionManager.RandomEnemyFaction();
                    }
                }
                if (sde.pawnFaction != null && Find.FactionManager.FirstFactionOfDef(sde.pawnFaction) != null)
                {
                    SelectedFaction = Find.FactionManager.FirstFactionOfDef(sde.pawnFaction);
                }else if (sde.anyHostile)
                {
                    SelectedFaction = Find.FactionManager.RandomEnemyFaction();
                }
                else
                {
                    SelectedFaction = Find.FactionManager.RandomNonHostileFaction();
                }

                if (sde.biome != null) map.TileInfo.PrimaryBiome = sde.biome;
                return;
            }

            map.TileInfo.PrimaryBiome = address.biome;
            map.TileInfo.temperature = address.temperature;
        });
        Find.World.pocketMaps.Add(mapParent);

        planetMap.listerThings.AllThings.ForEach(t =>
        {
            if (t.def.CanHaveFaction)
            {
                t.SetFaction(SelectedFaction);
            }
        });

        exitGate = planetMap.listerThings.ThingsOfDef(ArchoskipgateDefOf.GDFP_QuakkaaiExit).First() as Building_SkipGateExit;
        if(exitGate != null)
            exitGate.entryGate = entryGate;

        WorldComponent.AddNewAddressAndGate(CurrentGateAddress, planetMap);

        if (address.incidentToTrigger != null)
        {
            IncidentParms parms = StorytellerUtility.DefaultParmsNow(address.incidentToTrigger.category, planetMap);
            Find.Storyteller.incidentQueue.Add(new QueuedIncident(new FiringIncident(address.incidentToTrigger, null, parms), Find.TickManager.TicksGame + 300));
        }

        CurrentGateAddress = null;
        SelectedFaction = null;
    }

    public static List<StructureLayoutDef> GetRandomStructureLayouts()
    {
        List<StructureLayoutDef> layouts = DefDatabase<StructureLayoutDef>.AllDefsListForReading.Where(sld => sld.HasModExtension<StructureDefModExtension>()).Where(sld=>!sld.GetModExtension<StructureDefModExtension>().excludeFromRandomGen).ToList();

        StructureLayoutDef firstChoice = layouts.RandomElementWithFallback();
        if(firstChoice == null) return [];
        List<StructureLayoutDef> choices = [firstChoice];

        if(firstChoice.GetModExtension<StructureDefModExtension>().standalone) return choices;

        List<StructureLayoutDef> standaloneLayouts = layouts.Where(sld => sld.GetModExtension<StructureDefModExtension>().standalone).Except(firstChoice).ToList();

        choices.AddRange(standaloneLayouts.TakeRandomDistinct(new IntRange(1, 3).RandomInRange));

        return choices;
    }

    public string GetUniqueLoadID()
    {
        return "GDFP_GateAddress_" + name;
    }
}
