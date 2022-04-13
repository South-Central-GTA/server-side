using System.Text.Json;
using AltV.Net;

namespace Server.Database.Models.Delivery;

public class ProductDeliveryModel
    : DeliveryModel
{
    public ProductDeliveryModel()
    {
    }

    public ProductDeliveryModel(int companyGroupModelId, int amount)
    {
        OrderGroupModelId = companyGroupModelId;
        ProductsRemaining = amount;
        OrderedProducts = ProductsRemaining;
    }

    public int ProductsRemaining { get; set; }
    public int OrderedProducts { get; set; }

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

        writer.Name("status");
        writer.Value((int)Status);

        writer.Name("productsRemaining");
        writer.Value(ProductsRemaining);

        writer.Name("orderedProducts");
        writer.Value(OrderedProducts);

        writer.EndObject();
    }
}