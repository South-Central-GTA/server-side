using AltV.Net;

namespace Server.Data.Models;

public class InteriorData
    : IWritable
{
    public string Name { get; set; }
    public int Price { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("name");
        writer.Value(Name);

        writer.Name("price");
        writer.Value(Price);

        writer.Name("x");
        writer.Value(X);

        writer.Name("y");
        writer.Value(Y);

        writer.Name("z");
        writer.Value(Z);

        writer.EndObject();
    }
}