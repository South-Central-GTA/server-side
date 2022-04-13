using AltV.Net.Data;

namespace Server.Data.Models;

public class LocationData
{
    public LocationData(Position position, Rotation rotation)
    {
        Position = position;
        Rotation = rotation;
    }

    public Position Position { get; set; }
    public Rotation Rotation { get; set; }
}