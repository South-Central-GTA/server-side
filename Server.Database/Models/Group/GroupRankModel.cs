using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Group;

public class GroupRankModel
    : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int GroupModelId { get; set; }

    public GroupModel GroupModel { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Level { get; set; }

    public string Name { get; set; } = "";

    public GroupPermission GroupPermission { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("groupId");
        writer.Value(GroupModelId);

        writer.Name("level");
        writer.Value(Level);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("groupPermission");
        writer.Value((int)GroupPermission);

        writer.EndObject();
    }
}