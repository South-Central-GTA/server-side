using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using AltV.Net.Data;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Housing;

public class DoorModel : PositionModelBase, ILockableEntity, IWritable
{
    public DoorModel()
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

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(DoorModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("hash");
        writer.Value(model.Hash);

        writer.Name("locked");
        writer.Value(model.LockState == LockState.CLOSED);

        writer.Name("positionX");
        writer.Value(model.PositionX);

        writer.Name("positionY");
        writer.Value(model.PositionY);

        writer.Name("positionZ");
        writer.Value(model.PositionZ);

        writer.EndObject();
    }
}