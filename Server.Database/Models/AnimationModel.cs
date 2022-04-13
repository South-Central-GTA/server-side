using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models;

public class AnimationModel
    : ModelBase, IWritable
{
    public AnimationModel(string name, string dictionary, string clip)
    {
        Dictionary = dictionary;
        Clip = clip;
        Name = name;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public string Dictionary { get; set; }
    public string Clip { get; set; }
    public string Name { get; set; }
    public AnimationFlag Flags { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("dictionary");
        writer.Value(Dictionary);

        writer.Name("clip");
        writer.Value(Clip);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("flags");
        writer.Value((int)Flags);

        writer.EndObject();
    }
}