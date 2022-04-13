using AltV.Net;

namespace Server.Data.Models.EntitySync;

public class MoveData
    : IWritable
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Speed { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("x");
        writer.Value(X);

        writer.Name("y");
        writer.Value(Y);

        writer.Name("z");
        writer.Value(Z);

        writer.Name("speed");
        writer.Value(Speed);

        writer.EndObject();
    }
}