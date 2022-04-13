using AltV.Net;

namespace Server.Data.Models.EntitySync;

public class RgbData
    : IWritable
{
    public RgbData(int red, int green, int blue)
    {
        Red = red;
        Green = green;
        Blue = blue;
    }

    public int Red { get; set; }
    public int Green { get; set; }
    public int Blue { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("red");
        writer.Value(Red);

        writer.Name("green");
        writer.Value(Green);

        writer.Name("blue");
        writer.Value(Blue);

        writer.EndObject();
    }
}