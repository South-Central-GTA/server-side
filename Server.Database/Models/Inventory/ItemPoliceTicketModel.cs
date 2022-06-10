using System.Text.Json;
using AltV.Net;

namespace Server.Database.Models.Inventory;

public class ItemPoliceTicketModel : ItemModel, IWritable
{
    public string ReferenceId { get; set; }
    public string Reason { get; set; }
    public int Costs { get; set; }
    public bool Payed { get; set; }
    public string CreatorCharacterName { get; set; }
    public int TargetCharacterId { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(ItemPoliceTicketModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("referenceId");
        writer.Value(model.ReferenceId);

        writer.Name("reason");
        writer.Value(model.Reason);

        writer.Name("costs");
        writer.Value(model.Costs);

        writer.Name("payed");
        writer.Value(model.Payed);

        writer.Name("creatorCharacterName");
        writer.Value(model.CreatorCharacterName);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.EndObject();
    }
}