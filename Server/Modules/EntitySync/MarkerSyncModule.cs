using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Enums.EntitySync;

namespace Server.Modules.EntitySync;

public class MarkerSyncModule : ISingletonScript
{
    private readonly Dictionary<IColShape, ServerMarker> _markers = new();

    public ServerMarker Create(MarkerType type, Position position, Vector3 direction, Vector3 rotation, Vector3 scale,
        Rgba color, int dimension, bool bobUpDown = false, uint streamRange = 200, string text = "",
        string ownerName = "", string createdAtJson = "")
    {
        var shape = Alt.CreateColShapeSphere(position, 1.0f);
        var obj = new ServerMarker(position, dimension, streamRange)
        {
            MarkerType = type,
            Rotation = rotation,
            Direction = direction,
            Scale = scale,
            Color = color,
            ColShape = shape,
            BobUpDown = bobUpDown,
            Text = text,
            OwnerName = ownerName,
            CreatedAtJson = createdAtJson
        };

        AltEntitySync.AddEntity(obj);
        _markers.Add(shape, obj);

        return obj;
    }

    public bool Delete(ulong objectId)
    {
        var serverMarker = Get(objectId);
        if (serverMarker == null)
        {
            return false;
        }

        _markers.Remove(serverMarker.ColShape);
        AltEntitySync.RemoveEntity(serverMarker);

        return true;
    }


    public void DeleteAll()
    {
        foreach (var serverMarker in GetAll())
        {
            AltEntitySync.RemoveEntity(serverMarker);
        }

        _markers.Clear();
    }

    public ServerMarker? Get(ulong objectId)
    {
        if (!AltEntitySync.TryGetEntity(objectId, (ulong)EntityType.MARKER, out var entity))
        {
            return null;
        }

        return entity is not ServerMarker serverMarker ? default : serverMarker;
    }

    public List<ServerMarker> GetAll()
    {
        return _markers.Select(entity => entity.Value).ToList();
    }

    public ServerMarker? GetMarker(IColShape colShape)
    {
        if (!_markers.ContainsKey(colShape))
        {
            return null;
        }

        if (!AltEntitySync.TryGetEntity(_markers[colShape].Id, (ulong)EntityType.MARKER, out var entity))
        {
            return null;
        }

        return entity is not ServerMarker serverMarker ? default : serverMarker;
    }
}