using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Group;
using Server.Database.Models.Inventory;

namespace Server.Database.Models.Housing;

public class HouseModel : PositionRotationModelBase, ILockableEntity, IWritable
{
    public HouseModel()
    {
    }

    public HouseModel(float x, float y, float z, float roll, float pitch, float yaw, int interiorId, int houseNumber,
        int price, string subName, int streetDirection)
    {
        PositionX = x;
        PositionY = y;
        PositionZ = z;
        Roll = roll;
        Pitch = pitch;
        Yaw = yaw;
        InteriorId = interiorId;
        HouseNumber = houseNumber;
        Price = price;
        SubName = subName;
        StreetDirection = streetDirection;

        Inventory = new InventoryModel { Name = "Hauslager", InventoryType = InventoryType.HOUSE, MaxWeight = 100 };
    }

    protected HouseModel(float x, float y, float z, float roll, float pitch, float yaw, int price, string subName)
    {
        PositionX = x;
        PositionY = y;
        PositionZ = z;
        Roll = roll;
        Pitch = pitch;
        Yaw = yaw;
        InteriorId = -1;
        HouseNumber = -1;
        Price = price;
        SubName = subName;
        StreetDirection = -1;

        Inventory = new InventoryModel { Name = "Hauslager", InventoryType = InventoryType.HOUSE, MaxWeight = 100 };
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    /// <summary>
    ///     Value has to be set before using on client side. Use points module to calculate the points.
    /// </summary>
    [NotMapped]
    public int SouthCentralPoints { get; set; }

    public int? CharacterModelId { get; set; }

    public int? GroupModelId { get; set; }
    public GroupModel? GroupModel { get; set; }

    public InventoryModel? Inventory { get; set; }

    public HouseType HouseType { get; set; }
    public int HouseNumber { get; set; }
    public string SubName { get; set; }
    public int StreetDirection { get; set; }
    public int Price { get; set; }

    public int? InteriorId { get; set; }


    public bool Rentable { get; set; }
    public bool BlockedOwnership { get; set; }
    public int? RentBankAccountId { get; set; }
    public List<int> Keys { get; set; } = new();
    public List<DoorModel> Doors { get; set; } = new();

    [NotMapped] public bool HasOwner => CharacterModelId.HasValue || GroupModelId.HasValue;

    [NotMapped] public bool HasNoOwner => !CharacterModelId.HasValue && !GroupModelId.HasValue;

    public LockState LockState { get; set; }

    public virtual void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(HouseModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("southCentralPoints");
        writer.Value(model.SouthCentralPoints);

        writer.Name("ownerId");
        writer.Value(model.CharacterModelId ?? -1);

        writer.Name("groupOwnerId");
        writer.Value(model.GroupModelId ?? -1);

        writer.Name("houseNumber");
        writer.Value(model.HouseNumber);

        writer.Name("subName");
        writer.Value(model.SubName);

        writer.Name("houseType");
        writer.Value((int)model.HouseType);

        writer.Name("streetDirection");
        writer.Value(model.StreetDirection);

        writer.Name("price");
        writer.Value(model.Price);

        writer.Name("interiorId");
        writer.Value(model.InteriorId ?? -1);

        writer.Name("lockState");
        writer.Value((int)model.LockState);

        writer.Name("roll");
        writer.Value(model.Roll);

        writer.Name("pitch");
        writer.Value(model.Pitch);

        writer.Name("yaw");
        writer.Value(model.Yaw);

        writer.Name("positionX");
        writer.Value(model.PositionX);

        writer.Name("positionY");
        writer.Value(model.PositionY);

        writer.Name("positionZ");
        writer.Value(model.PositionZ);

        writer.Name("rentable");
        writer.Value(model.Rentable);

        writer.Name("blockedOwnership");
        writer.Value(model.BlockedOwnership);

        writer.Name("keyItemIds");
        writer.Value(model.Keys.Count != 0 ? string.Join(", ", model.Keys.ToArray()) : "Keine Schlüssel");

        writer.Name("doors");

        writer.BeginArray();

        foreach (var door in model.Doors)
        {
            DoorModel.Serialize(door, writer);
        }

        writer.EndArray();

        writer.EndObject();
    }
}