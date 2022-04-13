using AltV.Net;

namespace Server.Data.Models;

public class BlipData
    : IWritable
{
    public BlipData(int sprite, float positionX, float positionY, float positionZ, int color)
    {
        Sprite = sprite;
        PositionX = positionX;
        PositionY = positionY;
        PositionZ = positionZ;
        Color = color;
    }

    public int Sprite { get; set; }
    public float PositionX { get; set; }
    public float PositionY { get; set; }
    public float PositionZ { get; set; }
    public int Color { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("sprite");
        writer.Value(Sprite);

        writer.Name("positionX");
        writer.Value(PositionX);

        writer.Name("positionY");
        writer.Value(PositionY);

        writer.Name("positionZ");
        writer.Value(PositionZ);

        writer.Name("color");
        writer.Value(Color);

        writer.EndObject();
    }
}