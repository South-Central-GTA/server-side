using System.Text.Json;
using AltV.Net;

namespace Server.Database.Models.Inventory;

public class ItemWeaponAttachmentModel
    : ItemModel
{
    public int? ItemWeaponId { get; set; }
    public ItemWeaponModel? ItemModelWeapon { get; set; }

    public override void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("catalogItemName");
        writer.Value(CatalogItemModelId.ToString());

        writer.Name("catalogItem");

        writer.BeginObject();

        writer.Name("id");
        writer.Value((int)CatalogItemModel.Id);

        writer.Name("name");
        writer.Value(CatalogItemModel.Name);

        writer.Name("image");
        writer.Value(CatalogItemModel.Image);

        writer.Name("description");
        writer.Value(CatalogItemModel.Description);

        writer.Name("useValue");
        writer.Value(CatalogItemModel.UseValue);

        writer.Name("rarity");
        writer.Value((int)CatalogItemModel.Rarity);

        writer.Name("weight");
        writer.Value(CatalogItemModel.Weight);

        writer.Name("stackable");
        writer.Value(CatalogItemModel.Stackable);

        writer.Name("buyable");
        writer.Value(CatalogItemModel.Buyable);

        writer.Name("sellable");
        writer.Value(CatalogItemModel.Sellable);

        writer.Name("price");
        writer.Value(CatalogItemModel.Price);

        writer.Name("sellPrice");
        writer.Value(CatalogItemModel.SellPrice);

        writer.EndObject();

        writer.Name("slot");
        writer.Value(Slot ?? -1);

        writer.Name("droppedByCharacter");
        writer.Value(DroppedByCharacter ?? "Unbekannt");

        writer.Name("customData");
        writer.Value(CustomData);

        writer.Name("note");
        writer.Value(Note);

        writer.Name("amount");
        writer.Value(Amount);

        writer.Name("condition");
        writer.Value(Condition ?? -1);

        writer.Name("isBought");
        writer.Value(IsBought);

        writer.Name("itemState");
        writer.Value((int)ItemState);

        writer.Name("positionX");
        writer.Value(PositionX);

        writer.Name("positionY");
        writer.Value(PositionY);

        writer.Name("positionZ");
        writer.Value(PositionZ);

        writer.Name("lastUsage");
        writer.Value(JsonSerializer.Serialize(LastUsage));

        writer.Name("attachedToWeaponItem");
        writer.Value(ItemWeaponId ?? -1);

        writer.EndObject();
    }
}