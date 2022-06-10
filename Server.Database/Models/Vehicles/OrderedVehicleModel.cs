using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Vehicles;

public class OrderedVehicleModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public string OrderedBy { get; set; }
    public string CatalogVehicleModelId { get; set; }
    public CatalogVehicleModel? CatalogVehicleModel { get; set; }

    public int GroupModelId { get; set; }

    public DateTime DeliverdAt { get; set; }
    public DateTime DeliveryRequestedAt { get; set; }
    public string DeliveryRequestedBy { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(OrderedVehicleModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("orderedBy");
        writer.Value(model.OrderedBy);

        writer.Name("model");
        writer.Value(model.CatalogVehicleModel.Model);

        writer.Name("displayName");
        writer.Value(model.CatalogVehicleModel.DisplayName);

        writer.Name("displayClass");
        writer.Value(model.CatalogVehicleModel.DisplayClass);

        writer.Name("deliverdAtJson");
        writer.Value(JsonSerializer.Serialize(model.DeliverdAt));

        writer.Name("deliveryRequestedAtJson");
        writer.Value(JsonSerializer.Serialize(model.DeliveryRequestedAt));

        writer.Name("deliveryRequestedBy");
        writer.Value(model.DeliveryRequestedBy);

        writer.EndObject();
    }
}