using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Vehicles;

public class OrderedVehicleModel
    : ModelBase, IWritable
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
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("orderedBy");
        writer.Value(OrderedBy);

        writer.Name("model");
        writer.Value(CatalogVehicleModel.Model);

        writer.Name("displayName");
        writer.Value(CatalogVehicleModel.DisplayName);

        writer.Name("displayClass");
        writer.Value(CatalogVehicleModel.DisplayClass);

        writer.Name("deliverdAt");
        writer.Value(JsonSerializer.Serialize(DeliverdAt));

        writer.Name("deliveryRequestedAt");
        writer.Value(JsonSerializer.Serialize(DeliveryRequestedAt));

        writer.Name("deliveryRequestedBy");
        writer.Value(DeliveryRequestedBy);

        writer.EndObject();
    }
}