using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Enums;

namespace Server.Database.Models.Housing;

public class LeaseCompanyHouseModel
    : HouseModel
{
    public LeaseCompanyHouseModel()
    {
    }

    public LeaseCompanyHouseModel(LeaseCompanyType leaseCompanyType, float positionX, float positionY, float positionZ,
                                  float rotationRoll, float rotationPitch, float rotationYaw, int price, string subName,
                                  bool rentable = true)
        : base(positionX, positionY, positionZ, rotationRoll, rotationPitch, rotationYaw, price, subName)
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

        writer.Name("leaseCompanyType");
        writer.Value((int)LeaseCompanyType);

        writer.Name("playerDuty");
        writer.Value(PlayerDuty);

        writer.Name("cashierX");
        writer.Value(CashierX ?? 0);

        writer.Name("cashierY");
        writer.Value(CashierY ?? 0);

        writer.Name("cashierZ");
        writer.Value(CashierZ ?? 0);

        writer.Name("cashierHeading");
        writer.Value(CashierHeading ?? 0);

        writer.Name("rentable");
        writer.Value(Rentable);

        writer.Name("blockedOwnership");
        writer.Value(BlockedOwnership);

        writer.EndObject();
    }
}