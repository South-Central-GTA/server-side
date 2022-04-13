using AltV.Net;
using Server.Database.Enums;

namespace Server.Data.Models;

public class AnimationOptions
    : IWritable
{
    public int Speed { get; set; } = 3;
    public int SpeedMultiplier { get; set; } = -8;
    public int Duration { get; set; } = -1;
    public AnimationFlag Flag { get; set; }
    public int PlaybackRate { get; set; }
    public bool LockX { get; set; }
    public bool LockY { get; set; }
    public bool LockZ { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("speed");
        writer.Value(Speed);

        writer.Name("speedMultiplier");
        writer.Value(SpeedMultiplier);

        writer.Name("duration");
        writer.Value(Duration);

        writer.Name("flag");
        writer.Value((int)Flag);

        writer.Name("playbackRate");
        writer.Value(PlaybackRate);

        writer.Name("lockX");
        writer.Value(LockX);

        writer.Name("lockY");
        writer.Value(LockY);

        writer.Name("lockZ");
        writer.Value(LockZ);

        writer.EndObject();
    }
}