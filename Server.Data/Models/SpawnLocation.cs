using System.Collections.Generic;
using AltV.Net;
using AltV.Net.Data;

namespace Server.Data.Models;

public class SpawnLocation : IWritable
{
    public SpawnLocation(int id, string name, Position position, Rotation rotation, List<LocationData> vehicleLocations)
    {
        Id = id;
        Name = name;
        X = position.X;
        Y = position.Y;
        Z = position.Z;
        Pitch = rotation.Pitch;
        Roll = rotation.Roll;
        Yaw = rotation.Yaw;
        VehicleLocations = vehicleLocations;
    }

    public int Id { get; set; }
    public string Name { get; set; }
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float Pitch { get; set; }
    public float Roll { get; set; }
    public float Yaw { get; set; }
    public List<LocationData> VehicleLocations { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("x");
        writer.Value(X);

        writer.Name("y");
        writer.Value(Y);

        writer.Name("z");
        writer.Value(Z);

        writer.EndObject();
    }
}