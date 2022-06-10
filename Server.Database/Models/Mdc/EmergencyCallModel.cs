using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Mdc;

public class EmergencyCallModel : ModelBase, IWritable
{
    public EmergencyCallModel()
    {
    }

    public EmergencyCallModel(string phoneNumber, FactionType factionType, string situation, string location)
    {
        PhoneNumber = phoneNumber;
        Situation = situation;
        Location = location;
        FactionType = factionType;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public string PhoneNumber { get; set; }
    public string Location { get; set; } = "";
    public string Situation { get; set; } = "";
    public FactionType FactionType { get; set; }


    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(EmergencyCallModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("phoneNumber");
        writer.Value(model.PhoneNumber);

        writer.Name("situation");
        writer.Value(model.Situation);

        writer.Name("location");
        writer.Value(model.Location);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}