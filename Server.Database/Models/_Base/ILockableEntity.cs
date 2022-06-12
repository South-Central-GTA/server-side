using Server.Database.Enums;

namespace Server.Database.Models._Base;

public interface ILockableEntity
{
    public int Id { get; init; }
    public LockState LockState { get; set; }
    public float PositionX { get; set; }

    public float PositionY { get; set; }

    public float PositionZ { get; set; }
}