using System.Numerics;
using AltV.Net.EntitySync;
using Server.Data.Enums.EntitySync;

namespace Server.Core.Entities;

public class ServerDoor : Entity
{
    public ServerDoor(Vector3 position, int dimension, uint range) : base((ulong)EntityType.DOOR, position, dimension, range)
    {
    }

    public uint Hash
    {
        get => !TryGetData("hash", out uint hash) ? 0 : hash;
        set => SetData("hash", value);
    }

    public float Heading
    {
        get => !TryGetData("heading", out float heading) ? 0 : heading;
        set => SetData("heading", value);
    }

    /// <summary>
    ///     Set the lock status of the door
    /// </summary>
    public bool? Locked
    {
        get => TryGetData("locked", out bool locked) && locked;
        set => SetData("locked", value);
    }

    public int HouseId { get; set; }
}