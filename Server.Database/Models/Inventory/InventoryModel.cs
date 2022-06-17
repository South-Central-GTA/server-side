using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;
using Server.Database.Models.Housing;
using Server.Database.Models.Vehicles;

namespace Server.Database.Models.Inventory;

public class InventoryModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int? CharacterModelId { get; set; }
    public CharacterModel? CharacterModel { get; set; }

    public int? HouseModelId { get; set; }
    public HouseModel? HouseModel { get; set; }

    public int? VehicleModelId { get; set; }
    public PlayerVehicleModel? VehicleModel { get; set; }

    public int? ItemClothModelId { get; set; }
    public ItemClothModel? ItemClothModel { get; set; }

    public int? GroupCharacterId { get; set; }
    public int? GroupId { get; set; }

    public InventoryType InventoryType { get; set; }

    public List<ItemModel> Items { get; init; } = new();

    public float MaxWeight { get; set; }
    public string Name { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(InventoryModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("name");
        writer.Value(model.Name);

        writer.Name("inventoryType");
        writer.Value((int)model.InventoryType);

        writer.Name("items");

        writer.BeginArray();

        foreach (var item in model.Items)
        {
            switch (item)
            {
                case ItemClothModel itemClothModel:
                    ItemClothModel.Serialize(itemClothModel, writer);
                    break;
                case ItemPhoneModel itemPhoneModel:
                    ItemPhoneModel.Serialize(itemPhoneModel, writer);
                    break;
                case ItemPoliceTicketModel itemPoliceTicketModel:
                    ItemPoliceTicketModel.Serialize(itemPoliceTicketModel, writer);
                    break;
                case ItemWeaponAttachmentModel itemWeaponAttachmentModel:
                    ItemWeaponAttachmentModel.Serialize(itemWeaponAttachmentModel, writer);
                    break;
                default:
                    ItemModel.Serialize(item, writer);
                    break;
            }
        }

        writer.EndArray();

        writer.Name("maxWeight");
        writer.Value(model.MaxWeight);

        writer.EndObject();
    }
}