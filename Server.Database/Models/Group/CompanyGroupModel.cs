using System.Collections.Generic;
using AltV.Net;
using Server.Database.Enums;

namespace Server.Database.Models.Group;

public class CompanyGroupModel
    : GroupModel
{
    public CompanyGroupModel()
    {
    }

    public CompanyGroupModel(string name)
        : base(name)
    {
        GroupType = GroupType.COMPANY;

        Ranks = new List<GroupRankModel> { new() { GroupModelId = Id, Name = "Member", Level = 1 } };
    }

    public LicensesFlags LicensesFlags { get; set; }
    public int PurchasedLicenses { get; set; }
    public int Products { get; set; }
    public VisiblityState DeliveryVisibilityStatus { get; set; }

    public override void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("licenses");
        writer.Value((int)LicensesFlags);

        writer.Name("products");
        writer.Value(Products);

        writer.Name("deliveryVisibilityStatus");
        writer.Value((int)DeliveryVisibilityStatus);

        writer.Name("id");
        writer.Value(Id);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("status");
        writer.Value((int)Status);

        writer.Name("groupType");
        writer.Value((int)GroupType);

        writer.Name("members");
        writer.BeginArray();

        if (Members != null)
        {
            for (var i = 0; i < Members.Count; i++)
            {
                writer.BeginObject();

                var member = Members[i];

                writer.Name("groupId");
                writer.Value(member.GroupModelId);

                writer.Name("characterId");
                writer.Value(member.CharacterModelId);

                writer.Name("characterName");
                writer.Value(member.CharacterModel.Name);

                writer.Name("level");
                writer.Value(member.RankLevel);

                writer.Name("salary");
                writer.Value(member.Salary);

                writer.Name("bankAccountId");
                writer.Value(member.BankAccountId);

                writer.Name("owner");
                writer.Value(member.Owner);

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.Name("ranks");
        writer.BeginArray();

        if (Ranks != null)
        {
            for (var i = 0; i < Ranks.Count; i++)
            {
                writer.BeginObject();

                var rank = Ranks[i];

                writer.Name("groupId");
                writer.Value(rank.GroupModelId);

                writer.Name("level");
                writer.Value(rank.Level);

                writer.Name("name");
                writer.Value(rank.Name);

                writer.Name("groupPermission");
                writer.Value((int)rank.GroupPermission);

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.Name("houses");
        writer.BeginArray();

        if (Houses != null)
        {
            for (var i = 0; i < Houses.Count; i++)
            {
                writer.BeginObject();

                var house = Houses[i];

                writer.Name("id");
                writer.Value(house.Id);

                writer.Name("southCentralPoints");
                writer.Value(house.SouthCentralPoints);

                writer.Name("ownerId");
                writer.Value(house.CharacterModelId ?? -1);

                writer.Name("groupOwnerId");
                writer.Value(house.GroupModelId ?? -1);

                writer.Name("houseNumber");
                writer.Value(house.HouseNumber);

                writer.Name("subName");
                writer.Value(house.SubName);

                writer.Name("streetDirection");
                writer.Value(house.StreetDirection);

                writer.Name("price");
                writer.Value(house.Price);

                writer.Name("interiorId");
                writer.Value(house.InteriorId ?? -1);

                writer.Name("lockState");
                writer.Value((int)house.LockState);

                writer.Name("roll");
                writer.Value(house.Roll);

                writer.Name("pitch");
                writer.Value(house.Pitch);

                writer.Name("yaw");
                writer.Value(house.Yaw);

                writer.Name("positionX");
                writer.Value(house.PositionX);

                writer.Name("positionY");
                writer.Value(house.PositionY);

                writer.Name("positionZ");
                writer.Value(house.PositionZ);

                writer.EndObject();
            }
        }

        writer.EndArray();

        writer.EndObject();
    }
}