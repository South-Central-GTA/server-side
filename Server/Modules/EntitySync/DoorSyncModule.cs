using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.EntitySync;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Database.Enums;
using Server.Database.Models.Housing;

namespace Server.Modules.EntitySync;

public class DoorSyncModule : ISingletonScript
{
    private readonly Dictionary<ulong, ServerDoor> _doors = new();

    public ServerDoor Create(uint hash, Position position, float heading, bool locked, int houseId,
                             uint streamRange = 25)
    {
        var obj = new ServerDoor(position, 0, streamRange)
        {
            Hash = hash, Locked = locked, Heading = heading, HouseId = houseId
        };

        AltEntitySync.AddEntity(obj);
        _doors.TryAdd(obj.Id, obj);

        return obj;
    }

    public bool Delete(ulong objectId)
    {
        var serverDoor = Get(objectId);
        if (serverDoor == null)
        {
            return false;
        }

        _doors.Remove(serverDoor.Id);
        AltEntitySync.RemoveEntity(serverDoor);

        return true;
    }

    public ServerDoor? Get(ulong objectId)
    {
        return _doors[objectId];
    }

    public List<ServerDoor> GetAllDoors()
    {
        return _doors.Select(entity => entity.Value).ToList();
    }

    public ServerDoor? GetClosestDoor(Position position)
    {
        if (GetAllDoors().Count == 0)
        {
            return null;
        }

        ServerDoor obj = null;
        float distance = 5000;

        foreach (var o in GetAllDoors())
        {
            var dist = position.Distance(o.Position);
            if (dist < distance)
            {
                obj = o;
                distance = dist;
            }
        }

        return obj;
    }

    public void UpdateHouseDoor(DoorModel doorModel)
    {
        var serverDoor =
            GetAllDoors().FirstOrDefault(d => d.Position == doorModel.Position && d.Hash == doorModel.Hash);
        if (serverDoor == null)
        {
            return;
        }

        serverDoor.Locked = doorModel.LockState == LockState.CLOSED;
    }
}