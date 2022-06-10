using System.Text.Json;
using AltV.Net;

namespace Server.Database.Models.Delivery;

public class ProductDeliveryModel : DeliveryModel
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
        Serialize(this, writer);
    }

    public static void Serialize(ProductDeliveryModel model, IMValueWriter writer)
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

        writer.Name("status");
        writer.Value((int)model.Status);

        writer.Name("productsRemaining");
        writer.Value(model.ProductsRemaining);

        writer.Name("orderedProducts");
        writer.Value(model.OrderedProducts);

        writer.EndObject();
    }
}