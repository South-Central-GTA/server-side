using System.ComponentModel.DataAnnotations.Schema;
using System.Numerics;

namespace Server.Database.Models._Base;

public abstract class PositionRotationModelBase
    : PositionModelBase
{
    public float Roll { get; set; }
    public float Pitch { get; set; }
    public float Yaw { get; set; }

    [NotMapped]
    public Vector3 Rotation
    {
        get => new(Roll, Pitch, Yaw);
        set
        {
            Roll = value.X;
            Pitch = value.Y;
            Yaw = value.Z;
        }
    }
}