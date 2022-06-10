using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;

namespace Server.Database.Models.Delivery;

public class VehicleDeliveryModel : DeliveryModel
{
    public VehicleDeliveryModel()
    {
    }

    public VehicleDeliveryModel(int companyGroupModelId, string vehicleModel, string displayName)
    {
        DeliveryType = DeliveryType.VEHICLES;
        OrderGroupModelId = companyGroupModelId;
        VehicleModel = vehicleModel;
        DisplayName = displayName;
    }

    public string VehicleModel { get; set; }
    public string DisplayName { get; set; }

    public override void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(VehicleDeliveryModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("deliveryType");
        writer.Value((int)model.DeliveryType);

        writer.Name("orderGroupId");
        writer.Value(model.OrderGroupModelId);

        writer.Name("orderGroupName");
        writer.Value(model.OrderGroupModel.Name);

        writer.Name("supplierGroupId");
        writer.Value(model.SupplierGroupModelId ?? -1);

        writer.Name("supplierGroupName");
        writer.Value(model.SupplierGroupModel != null ? model.SupplierGroupModel.Name : "");

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.Name("supplierFullName");
        writer.Value(model.SupplierFullName ?? string.Empty);

        writer.Name("supplierPhoneNumber");
        writer.Value(model.SupplierPhoneNumber ?? string.Empty);

        writer.Name("displayName");
        writer.Value(model.DisplayName);

        writer.Name("status");
        writer.Value((int)model.Status);

        writer.EndObject();
    }
}