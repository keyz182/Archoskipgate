using System;
using System.Collections.Generic;
using RimWorld.Planet;
using Verse;

namespace Archoskipgate;

public class GDFP_WorldComponent : WorldComponent
{
    public List<GateAddress> LearnedAddresses = new();

    public GDFP_WorldComponent(World world) : base(world)
    {
    }

    public void AddNewAddressAndGate(GateAddress address, Map map)
    {
        address.mapReference = map;
        GateAddress match = LearnedAddresses.FirstOrDefault(ga => ga.address == address.address);

        if (match != null)
        {

            int idx = LearnedAddresses.IndexOf(match);
            LearnedAddresses.RemoveAt(idx);
        }

        LearnedAddresses.Add(address);
    }
    //
    // public void RestoreMap(GateAddress address, Map parent)
    // {
    //
    // }
    //
    // public void MothballPocketMap(Map map)
    // {
    //     if (map is not { Parent: PocketMapParent parent })
    //         return;
    //
    //     GateAddress address = LearnedAddresses.FirstOrDefault(ga => ga.Map == map);
    //
    //     if (address == null)
    //         return;
    //
    //     address.map = map;
    //     address.mapReference = null;
    //
    //     Find.World.pocketMaps.Remove(parent);
    //
    //     try
    //     {
    //         map.powerNetManager.UpdatePowerNetsAndConnections_First();
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error("Error while deiniting map: could not execute power related tasks: " + (object) ex);
    //     }
    //
    //
    //     try
    //     {
    //         map.weatherManager.EndAllSustainers();
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error("Error while deiniting map: could not end all weather sustainers: " + (object) ex);
    //     }
    //
    //     try
    //     {
    //         Find.SoundRoot.sustainerManager.EndAllInMap(map);
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error("Error while deiniting map: could not end all effect sustainers: " + (object) ex);
    //     }
    //
    //
    //     try
    //     {
    //         map.deferredSpawner.Notify_MapRemoved();
    //     }
    //     catch (Exception ex)
    //     {
    //         Log.Error("Error while deiniting map and notifying DeferredSpawner: " + (object) ex);
    //     }
    //
    //
    //     // try
    //     // {
    //     //   MapDeiniter.NotifyEverythingWhichUsesMapReference(map);
    //     // }
    //     // catch (Exception ex)
    //     // {
    //     //   Log.Error("Error while deiniting map: could not notify things/regions/rooms/etc: " + (object) ex);
    //     // }
    //
    //
    //     // try
    //     // {
    //     //   Find.Archive.Notify_MapRemoved(map);
    //     // }
    //     // catch (Exception ex)
    //     // {
    //     //   Log.Error("Error while deiniting map: could not remove look targets: " + (object) ex);
    //     // }
    //     //
    //     // try
    //     // {
    //     //   Find.Storyteller.incidentQueue.Notify_MapRemoved(map);
    //     // }
    //     // catch (Exception ex)
    //     // {
    //     //   Log.Error("Error while deiniting map: could not remove queued incidents: " + (object) ex);
    //     // }
    //
    //     Find.World.renderer.wantedMode = WorldRenderMode.None;
    // }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Collections.Look(ref LearnedAddresses, "LearnedAddresses", LookMode.Deep);
    }
}
