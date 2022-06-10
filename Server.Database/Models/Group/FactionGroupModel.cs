using System.Collections.Generic;
using Server.Database.Enums;

namespace Server.Database.Models.Group;

public class FactionGroupModel : GroupModel
{
    public FactionGroupModel()
    {
    }

    public FactionGroupModel(string name, FactionType factionType) : base(name)
    {
        GroupType = GroupType.FACTION;
        FactionType = factionType;

        Ranks = new List<GroupRankModel> { new() { GroupModelId = Id, Level = 1, Name = "Member" } };
    }

    public FactionType FactionType { get; set; }
}