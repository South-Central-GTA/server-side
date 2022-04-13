using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Housing;

namespace Server.Database.Models.Group;

public class GroupModel
    : ModelBase, IWritable
{
    public GroupModel()
    {
    }

    public GroupModel(string name)
    {
        Name = name;
        MaxRanks = 20;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public GroupState Status { get; set; }
    public string Name { get; set; }
    public GroupType GroupType { get; set; }

    // Settings
    public int MaxRanks { get; set; }

    public List<GroupMemberModel> Members { get; init; } = new();
    public List<GroupRankModel> Ranks { get; init; } = new();
    public List<HouseModel> Houses { get; init; } = new();

    public virtual void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

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