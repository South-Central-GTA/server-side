using System.Collections.Generic;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models.Housing;

namespace Server.Database.Models.Group;

public class CompanyGroupModel : GroupModel
{
    public CompanyGroupModel()
    {
        
    }
    
    public CompanyGroupModel(string name) : base(name)
    {
        GroupType = GroupType.COMPANY;

        Ranks = new List<GroupRankModel> { new() { GroupModelId = Id, Name = "Member", Level = 1 } };
    }

    public LicensesFlags LicensesFlags { get; set; }
    public int PurchasedLicenses { get; set; }
    public int Products { get; set; }
    public VisiblityState DeliveryVisibilityStatus { get; set; }

    public ulong? MarkerId { get; set; }
    public float? VehicleInteractionPointX { get; set; }
    public float? VehicleInteractionPointY { get; set; }
    public float? VehicleInteractionPointZ { get; set; }
    public float? VehicleInteractionPointRoll { get; set; }
    public float? VehicleInteractionPointPitch { get; set; }
    public float? VehicleInteractionPointYaw { get; set; }

    public override void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(CompanyGroupModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("licenses");
        writer.Value((int)model.LicensesFlags);

        writer.Name("products");
        writer.Value(model.Products);

        writer.Name("deliveryVisibilityStatus");
        writer.Value((int)model.DeliveryVisibilityStatus);

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("name");
        writer.Value(model.Name);

        writer.Name("status");
        writer.Value((int)model.Status);

        writer.Name("groupType");
        writer.Value((int)model.GroupType);

        writer.Name("members");
        writer.BeginArray();

        foreach (var member in model.Members)
        {
            GroupMemberModel.Serialize(member, writer);
        }

        writer.EndArray();

        writer.Name("ranks");
        writer.BeginArray();

        foreach (var rank in model.Ranks)
        {
            GroupRankModel.Serialize(rank, writer);
        }

        writer.EndArray();

        writer.Name("houses");
        writer.BeginArray();

        foreach (var house in model.Houses)
        {
            HouseModel.Serialize(house, writer);
        }

        writer.EndArray();

        writer.EndObject();
    }
}