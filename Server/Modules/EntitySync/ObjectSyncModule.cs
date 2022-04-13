using System.Collections.Generic;
using System.Linq;
using AltV.Net.Data;
using AltV.Net.EntitySync;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Enums.EntitySync;

namespace Server.Modules.EntitySync;

public class ObjectSyncModule : ISingletonScript
{
    private readonly Dictionary<ulong, ServerObject> _objects = new();

    public ServerObject Create(string model, string name, Position position, Rotation rotation, int dimension, uint streamRange = 200,
                               bool freeze = false, bool onFire = false, int itemId = -1, string ownerName = "", string createdAtJson = "")
    {
        var obj = new ServerObject(position, dimension, streamRange)
        {
            Model = model,
            Name = name,
            Freeze = freeze,
            OnFire = onFire,
            ItemId = itemId,
            Rotation = rotation,
            OwnerName = ownerName,
            CreatedAtJson = createdAtJson
        };

        AltEntitySync.AddEntity(obj);
        _objects.Add(obj.Id, obj);

        return obj;
    }

    public bool Delete(ulong objectId)
    {
        var serverObject = Get(objectId);
        if (serverObject == null)
        {
            return false;
        }

        AltEntitySync.RemoveEntity(serverObject);
        _objects.Remove(objectId);
        
        return true;
    }

    public void DeleteAll()
    {
        foreach (var serverObject in GetAll())
        {
            AltEntitySync.RemoveEntity(serverObject);
        }

        _objects.Clear();
    }

    public ServerObject? Get(ulong objectId)
    {
        if (!AltEntitySync.TryGetEntity(objectId, (ulong)EntityType.OBJECT, out var entity))
        {
            return null;
        }

        return entity is not ServerObject serverObject ? default : serverObject;
    }
    
    public List<ServerObject> GetAll()
    {
        return _objects.Select(entity => entity.Value).ToList();
    }
}