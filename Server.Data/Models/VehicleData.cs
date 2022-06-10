using AltV.Net;

namespace Server.Data.Models;

public class VehicleData : IWritable
{
    public int Id { get; set; }
    public string Model { get; set; }
    public string DisplayName { get; set; }
    public string DisplayClass { get; set; }
    public int CharacterId { get; set; }
    public string CharacterName { get; set; }
    public string NumberPlateText { get; set; }
    public bool IsGroupVehicle { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(VehicleData vehicleData, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(vehicleData.Id);

        writer.Name("model");
        writer.Value(vehicleData.Model);

        writer.Name("displayName");
        writer.Value(vehicleData.DisplayName);

        writer.Name("displayClass");
        writer.Value(vehicleData.DisplayClass);

        writer.Name("characterId");
        writer.Value(vehicleData.CharacterId);

        writer.Name("characterName");
        writer.Value(vehicleData.CharacterName);

        writer.Name("isGroupVehicle");
        writer.Value(vehicleData.IsGroupVehicle);

        writer.Name("numberPlateText");
        writer.Value(vehicleData.NumberPlateText);

        writer.EndObject();
    }
}