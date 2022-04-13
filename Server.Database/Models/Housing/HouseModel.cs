using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Group;
using Server.Database.Models.Inventory;

namespace Server.Database.Models.Housing;

public class HouseModel
    : PositionRotationModelBase, ILockableEntity, IWritable
{
    protected HouseModel()
    {
    }

    public HouseModel(float x, float y, float z, float roll, float pitch, float yaw, int interiorId, int houseNumber, int price, string subName, int streetDirection)
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

    public LockState LockState { get; set; }
    public List<int> Keys { get; set; } = new();
    public List<DoorModel> Doors { get; set; } = new();

    [NotMapped] 
    public bool HasOwner => CharacterModelId.HasValue || GroupModelId.HasValue;
    
    [NotMapped] 
    public bool HasNoOwner => !CharacterModelId.HasValue && !GroupModelId.HasValue;

    public virtual void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("southCentralPoints");
        writer.Value(SouthCentralPoints);

        writer.Name("ownerId");
        writer.Value(CharacterModelId ?? -1);

        writer.Name("groupOwnerId");
        writer.Value(GroupModelId ?? -1);

        writer.Name("houseNumber");
        writer.Value(HouseNumber);

        writer.Name("subName");
        writer.Value(SubName);

        writer.Name("houseType");
        writer.Value((int)HouseType);

        writer.Name("streetDirection");
        writer.Value(StreetDirection);

        writer.Name("price");
        writer.Value(Price);

        writer.Name("interiorId");
        writer.Value(InteriorId ?? -1);

        writer.Name("lockState");
        writer.Value((int)LockState);

        writer.Name("roll");
        writer.Value(Roll);

        writer.Name("pitch");
        writer.Value(Pitch);

        writer.Name("yaw");
        writer.Value(Yaw);

        writer.Name("positionX");
        writer.Value(PositionX);

        writer.Name("positionY");
        writer.Value(PositionY);

        writer.Name("positionZ");
        writer.Value(PositionZ);
        
        writer.Name("rentable");
        writer.Value(Rentable);
        
        writer.Name("blockedOwnership");
        writer.Value(BlockedOwnership);
        
        writer.Name("keyItemIds");
        writer.Value(Keys.Count != 0 ? string.Join(", ", Keys.ToArray()) : "Keine Schlüssel");
        
        writer.Name("doors");

        writer.BeginArray();

        foreach (var door in Doors)
        {
            writer.BeginObject();

            writer.Name("id");
            writer.Value(door.Id);

            writer.Name("hash");
            writer.Value(door.Hash);

            writer.Name("locked");
            writer.Value(door.LockState == LockState.CLOSED);

            writer.Name("positionX");
            writer.Value(door.PositionX);

            writer.Name("positionY");
            writer.Value(door.PositionY);

            writer.Name("positionZ");
            writer.Value(door.PositionZ);
            
            writer.EndObject();
        }
        
        writer.EndArray();

        writer.EndObject();
    }
}