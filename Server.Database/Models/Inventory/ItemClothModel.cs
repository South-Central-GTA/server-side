using AltV.Net;
using Server.Database.Enums;

namespace Server.Database.Models.Inventory;

public class ItemClothModel : ItemModel
{
    public InventoryModel? ClothingInventoryModel { get; set; }
    public GenderType GenderType { get; set; }
    public byte DrawableId { get; set; }
    public byte TextureId { get; set; }
    public string Title { get; set; } = string.Empty;

    public override void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(ItemClothModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("catalogItemName");
        writer.Value(model.CatalogItemModelId.ToString());

        writer.Name("catalogItem");

        CatalogItemModel.Serialize(model.CatalogItemModel, writer);

        writer.Name("slot");
        writer.Value(model.Slot ?? -1);

        writer.Name("customData");
        writer.Value(model.CustomData ?? string.Empty);

        writer.Name("note");
        writer.Value(model.Note ?? string.Empty);

        writer.Name("amount");
        writer.Value(model.Amount);

        writer.Name("isBought");
        writer.Value(model.IsBought);

        writer.Name("itemState");
        writer.Value((int)model.ItemState);

        writer.Name("genderType");
        writer.Value((int)model.GenderType);

        writer.Name("drawableId");
        writer.Value(model.DrawableId);

        writer.Name("textureId");
        writer.Value(model.TextureId);

        writer.Name("title");
        writer.Value(model.Title);

        writer.EndObject();
    }
}