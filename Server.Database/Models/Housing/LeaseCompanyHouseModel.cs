using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;

namespace Server.Database.Models.Housing;

public class LeaseCompanyHouseModel : HouseModel
{
    public LeaseCompanyHouseModel()
    {
    }

    public LeaseCompanyHouseModel(LeaseCompanyType leaseCompanyType, float positionX, float positionY, float positionZ,
        float rotationRoll, float rotationPitch, float rotationYaw, int price, string subName, bool rentable = true) :
        base(positionX, positionY, positionZ, rotationRoll, rotationPitch, rotationYaw, price, subName)
    {
        LeaseCompanyType = leaseCompanyType;
        HouseType = HouseType.COMPANY;
        Rentable = rentable;
        HasCashier = true;
    }

    public LeaseCompanyType LeaseCompanyType { get; set; }

    public bool HasCashier { get; set; }

    [NotMapped] public bool HasOpen => HasCashier || PlayerDuty;

    [NotMapped] public bool PlayerDuty => PlayerDuties > 0;

    public int PlayerDuties { get; set; }

    public float? CashierX { get; set; }
    public float? CashierY { get; set; }
    public float? CashierZ { get; set; }
    public float? CashierHeading { get; set; }

    public override void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(LeaseCompanyHouseModel model, IMValueWriter writer)
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

        writer.Name("leaseCompanyType");
        writer.Value((int)model.LeaseCompanyType);

        writer.Name("playerDuty");
        writer.Value(model.PlayerDuty);

        writer.Name("cashierX");
        writer.Value(model.CashierX ?? 0);

        writer.Name("cashierY");
        writer.Value(model.CashierY ?? 0);

        writer.Name("cashierZ");
        writer.Value(model.CashierZ ?? 0);

        writer.Name("cashierHeading");
        writer.Value(model.CashierHeading ?? 0);

        writer.Name("rentable");
        writer.Value(model.Rentable);

        writer.Name("blockedOwnership");
        writer.Value(model.BlockedOwnership);

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