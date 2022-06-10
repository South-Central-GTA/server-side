using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;
using Server.Database.Models.Group;
using Server.Database.Models.Inventory;

namespace Server.Database.Models.Vehicles;

public class PlayerVehicleModel : PositionRotationDimensionModelBase, ILockableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int? CharacterModelId { get; set; }
    public CharacterModel? CharacterModel { get; set; }

    public int? GroupModelOwnerId { get; set; }
    public GroupModel? GroupModelOwner { get; set; }

    public InventoryModel? InventoryModel { get; set; }

    public string Model { get; set; }

    public VehicleState VehicleState { get; set; }
    public int Price { get; set; }

    public string NumberplateText { get; set; } = "";

    public int EngineHealth { get; set; }
    public uint BodyHealth { get; set; }

    public int PrimaryColor { get; set; }
    public int SecondaryColor { get; set; }
    public byte Livery { get; set; }

    public float Fuel { get; set; }
    public float DrivenKilometre { get; set; }
    public List<string> LastDrivers { get; set; } = new();
    public bool EngineOn { get; set; }
    public List<int> Keys { get; set; } = new();


    public LockState LockState { get; set; }
}