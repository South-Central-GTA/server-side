using AltV.Net;

namespace Server.Data.Models;

public class VehicleData
    : IWritable
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
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("model");
        writer.Value(Model);

        writer.Name("displayName");
        writer.Value(DisplayName);

        writer.Name("displayClass");
        writer.Value(DisplayClass);

        writer.Name("characterId");
        writer.Value(CharacterId);

        writer.Name("characterName");
        writer.Value(CharacterName);

        writer.Name("isGroupVehicle");
        writer.Value(IsGroupVehicle);

        writer.Name("numberPlateText");
        writer.Value(NumberPlateText);

        writer.EndObject();
    }
}