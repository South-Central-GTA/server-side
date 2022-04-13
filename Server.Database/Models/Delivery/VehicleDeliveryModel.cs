using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;

namespace Server.Database.Models.Delivery;

public class VehicleDeliveryModel
    : DeliveryModel
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
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("deliveryType");
        writer.Value((int)DeliveryType);

        writer.Name("orderGroupId");
        writer.Value(OrderGroupModelId);

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

        writer.Name("displayName");
        writer.Value(DisplayName);

        writer.Name("status");
        writer.Value((int)Status);

        writer.EndObject();
    }
}