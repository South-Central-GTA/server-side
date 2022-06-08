using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net.Data;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Housing;

public class DoorModel
    : PositionModelBase, ILockableEntity
{
    protected DoorModel()
    {
    }

    public DoorModel(uint hash, Position position, int houseModelId)
    {
        Hash = hash;
        PositionX = position.X;
        PositionY = position.Y;
        PositionZ = position.Z;
        HouseModelId = houseModelId;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int HouseModelId { get; set; }
    public HouseModel HouseModel { get; set; }

    public uint Hash { get; set; }
    public LockState LockState { get; set; }
}