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

    public int Spoilers { get; set; }
    public int FrontBumper { get; set; }
    public int RearBumper { get; set; }
    public int SideSkirt { get; set; }
    public int Exhaust { get; set; }
    public int Frame { get; set; }
    public int Grille { get; set; }
    public int Hood { get; set; }
    public int Fender { get; set; }
    public int RightFender { get; set; }
    public int Roof { get; set; }
    public int Engine { get; set; }
    public int Brakes { get; set; }
    public int Transmission { get; set; }
    public int Horns { get; set; }
    public int Suspension { get; set; }
    public int Armor { get; set; }
    public int Turbo { get; set; }
    public int Xenon { get; set; }
    public int FrontWheels { get; set; }
    public int BackWheels { get; set; }
    public int PlateHolder { get; set; }
    public int PlateVanity { get; set; }
    public int TrimDesign { get; set; }
    public int Ornaments { get; set; }
    public int Dashboard { get; set; }
    public int DialDesign { get; set; }
    public int DoorSpeaker { get; set; }
    public int Seats { get; set; }
    public int SteeringWheel { get; set; }
    public int ShiftLever { get; set; }
    public int Plaques { get; set; }
    public int Speaker { get; set; }
    public int Trunk { get; set; }
    public int Hydraulics { get; set; }
    public int EngineBlock { get; set; }
    public int AirFilter { get; set; }
    public int Struts { get; set; }
    public int ArchCover { get; set; }
    public int Aerials { get; set; }
    public int Trim { get; set; }    
    public int Tank { get; set; }
    public int Windows { get; set; }
    public int Boost { get; set; }
    public int WindowTint { get; set; }
    public int Plate { get; set; }
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