using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI.Group;

namespace Archoskipgate;

public class PawnRepr : IExposable
{
    public class ThingRepr : IExposable
    {
        public ThingDef def;
        public int count;
        [CanBeNull] public ThingDef stuff;
        public string color;
        public QualityCategory quality;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref def, "def");
            Scribe_Values.Look(ref count, "count");
            Scribe_Defs.Look(ref stuff, "stuff");
            Scribe_Values.Look(ref color, "color");
            Scribe_Values.Look(ref quality, "quality");
        }

        public static ThingRepr FromThing(Thing thing)
        {
            if(thing == null) return null;
            ThingRepr thingRepr = new() {
                def = thing.def,
                count = thing.stackCount,
                stuff = thing.Stuff,
            };
            if (thing.TryGetComp(out CompQuality compQuality))
            {
                thingRepr.quality = compQuality.Quality;
            }

            if (thing.TryGetComp(out CompColorable compColorable))
            {
                thingRepr.color = compColorable.Color.ToString().Replace("RGBA", "");
            }

            return thingRepr;
        }

        public Thing ToThing()
        {
            Thing thing = ThingMaker.MakeThing(def, stuff);
            thing.stackCount = count;

            if (thing.TryGetComp(out CompQuality compQuality))
            {
                compQuality.SetQuality(quality, ArtGenerationContext.Outsider);
            }
            if ( thing.TryGetComp(out CompColorable compColorable))
            {
                Color? c = new Color?(ParseHelper.FromString<Color>(color));
                if(c.HasValue)
                    compColorable.SetColor(c.Value);
            }

            return thing;
        }
    }

    public PawnKindDef kindDef;
    public Name nameInt;
    public Gender gender;
    public bool FactionLeader;
    public int age;
    public List<ThingRepr> inventory;
    public ThingRepr equipment;
    public BeardDef beardDef;
    public TattooDef faceTattoo;
    public TattooDef bodyTattoo;
    public List<GeneDef> genes;
    public XenotypeDef xenotype;
    public IntVec3 spawnCell;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref kindDef, "kindDef");
        Scribe_Deep.Look(ref nameInt, "nameInt");
        Scribe_Values.Look(ref gender, "gender");
        Scribe_Values.Look(ref age, "age");
        Scribe_Collections.Look(ref inventory, "inventory", LookMode.Deep);
        Scribe_Deep.Look(ref equipment, "equipment");
        Scribe_Defs.Look(ref beardDef, "beardDef");
        Scribe_Defs.Look(ref faceTattoo, "faceTattoo");
        Scribe_Defs.Look(ref bodyTattoo, "bodyTattoo");
        Scribe_Collections.Look(ref genes, "genes", LookMode.Def);
        Scribe_Defs.Look(ref xenotype, "xenotype");
        Scribe_Values.Look(ref spawnCell, "spawnCell");
    }

    public ThingDefCountClass FromThing(Thing thing)
    {
        ThingDefCountClass thingDefCountClass = new(thing.def, thing.stackCount);
        if (thing.TryGetComp(out CompQuality compQuality))
        {
            thingDefCountClass.quality = compQuality.Quality;
        }

        if (thingDefCountClass.color.HasValue && thing.TryGetComp(out CompColorable compColorable))
        {
            thingDefCountClass.color = compColorable.Color;
        }

        return thingDefCountClass;
    }

    public Thing ToThing(ThingDefCountClass thingDefCountClass)
    {
        Thing thing = ThingMaker.MakeThing(thingDefCountClass.thingDef, thingDefCountClass.stuff);
        thing.stackCount = thingDefCountClass.count;
        if (thing.TryGetComp(out CompQuality compQuality))
        {
            compQuality.SetQuality(thingDefCountClass.quality, ArtGenerationContext.Outsider);
        }
        if (thingDefCountClass.color.HasValue && thing.TryGetComp(out CompColorable compColorable))
        {
            compColorable.SetColor(thingDefCountClass.color.Value);
        }

        return thing;
    }

    public static PawnRepr FromPawn(Pawn pawn)
    {
        PawnRepr repr = new()
        {
            kindDef = pawn.kindDef,
            nameInt = pawn.Name,
            gender = pawn.gender,
            age = pawn.ageTracker.AgeBiologicalYears,
            inventory = pawn.inventory.innerContainer.InnerListForReading.Select(ThingRepr.FromThing).ToList(),
            equipment = ThingRepr.FromThing(pawn.equipment.Primary),
            beardDef = pawn.style.beardDef,
            faceTattoo = pawn.style.FaceTattoo,
            bodyTattoo = pawn.style.BodyTattoo,
            genes = pawn.genes.GenesListForReading.Select(g=>g.def).ToList(),
            spawnCell = pawn.Position
        };

        return repr;
    }

    public bool SpawnPawn(Map map, Faction faction, Lord lord = null)
    {
        ModLog.Debug($"Attempring to spawn pawn {nameInt}");
        try
        {
            Pawn pawn = null;
            if (FactionLeader)
            {
                pawn = faction.leader;
            }
            if(pawn == null)
            {
                PawnGenerationRequest request = new(
                    kindDef,
                    faction,
                    PawnGenerationContext.NonPlayer,
                    forceGenerateNewPawn: true,
                    allowDead: false,
                    allowDowned: false,
                    inhabitant: false,
                    fixedBiologicalAge: age,
                    fixedChronologicalAge: age,
                    fixedGender: gender,
                    forcedEndogenes: genes,
                    forcedXenotype: xenotype,
                    dontGiveWeapon: true);

                pawn = PawnGenerator.GeneratePawn(request);
            }

            pawn.Name = nameInt;

            pawn.style.beardDef = beardDef;
            pawn.style.FaceTattoo = faceTattoo;
            pawn.style.BodyTattoo = bodyTattoo;

            lord?.AddPawn(pawn);

            foreach (ThingRepr thingRepr in inventory)
            {
                Thing thing = thingRepr.ToThing();
                pawn.inventory.innerContainer.TryAdd(thing);
            }

            if (equipment != null)
            {
                ThingWithComps thing = equipment.ToThing() as ThingWithComps;;
                pawn.equipment.AddEquipment(thing);
            }

            if (!spawnCell.InBounds(map))
            {
                RCellFinder.TryFindRandomCellNearWith(map.Center, (_)=>true, map, out spawnCell);
            }

            GenSpawn.Spawn(pawn, spawnCell, map);

            return true;
        }catch(Exception e)
        {
            ModLog.Error($"Failed to spawn pawn {nameInt} with exception {e}");
            return false;
        }
    }
}
