using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Housing;

namespace Server.Database.Models.Group;

public class GroupModel : ModelBase, IWritable
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
        Serialize(this, writer);
    }

    public static void Serialize(GroupModel model, IMValueWriter writer)
    {
        writer.BeginObject();

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