using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Server.Database.Models._Base;

public abstract class PositionModelBase
    : ModelBase
{
    public float PositionX { get; set; }

    public float PositionY { get; set; }

    public float PositionZ { get; set; }

    [NotMapped]
    public Vector3 Position
    {
        get => new(PositionX, PositionY, PositionZ);
        set
        {
            PositionX = value.X;
            PositionY = value.Y;
            PositionZ = value.Z;
        }
    }
}