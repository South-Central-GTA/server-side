using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Group;
using Server.Database.Models.Vehicles;

namespace Server.Database.Models.Delivery;

public class DeliveryModel
    : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public DeliveryType DeliveryType { get; set; }

    public int OrderGroupModelId { get; set; }
    public GroupModel OrderGroupModel { get; set; }

    public int? SupplierGroupModelId { get; set; }
    public GroupModel? SupplierGroupModel { get; set; }

    public int? SupplierCharacterId { get; set; }

    public string? SupplierPhoneNumber { get; set; }
    public string? SupplierFullName { get; set; }

    public int? PlayerVehicleModelId { get; set; }
    public PlayerVehicleModel? PlayerVehicleModel { get; set; }

    public DeliveryState OldStatus { get; set; }
    public DeliveryState Status { get; set; }

    public virtual void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("orderGroupId");
        writer.Value(OrderGroupModelId);

        writer.Name("deliveryType");
        writer.Value((int)DeliveryType);

        writer.Name("orderGroupName");
        writer.Value(OrderGroupModel.Name);

        writer.Name("supplierGroupId");
        writer.Value(SupplierGroupModelId ?? -1);

        writer.Name("supplierGroupName");
        writer.Value(SupplierGroupModel != null ? SupplierGroupModel.Name : "");

        writer.Name("createdAt");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.Name("supplierFullName");
        writer.Value(SupplierFullName);

        writer.Name("supplierPhoneNumber");
        writer.Value(SupplierPhoneNumber);

        writer.Name("status");
        writer.Value((int)Status);

        writer.EndObject();
    }
}