using System.Text.Json;
using AltV.Net;

namespace Server.Database.Models.Inventory;

public class ItemWeaponAttachmentModel : ItemModel
{
    public int? ItemWeaponId { get; set; }
    public ItemWeaponModel? ItemModelWeapon { get; set; }

    public override void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(ItemWeaponAttachmentModel model, IMValueWriter writer)
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

        writer.Name("droppedByCharacter");
        writer.Value(model.DroppedByCharacter ?? "Unbekannt");

        writer.Name("customData");
        writer.Value(model.CustomData ?? string.Empty);

        writer.Name("note");
        writer.Value(model.Note ?? string.Empty);

        writer.Name("amount");
        writer.Value(model.Amount);

        writer.Name("condition");
        writer.Value(model.Condition ?? -1);

        writer.Name("isBought");
        writer.Value(model.IsBought);

        writer.Name("itemState");
        writer.Value((int)model.ItemState);

        writer.Name("positionX");
        writer.Value(model.PositionX);

        writer.Name("positionY");
        writer.Value(model.PositionY);

        writer.Name("positionZ");
        writer.Value(model.PositionZ);

        writer.Name("lastUsageJson");
        writer.Value(JsonSerializer.Serialize(model.LastUsage));

        writer.Name("attachedToWeaponItem");
        writer.Value(model.ItemWeaponId ?? -1);

        writer.EndObject();
    }
}