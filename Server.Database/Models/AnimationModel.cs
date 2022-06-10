using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models;

public class AnimationModel : ModelBase, IWritable
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
        Serialize(this, writer);
    }

    public static void Serialize(AnimationModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("dictionary");
        writer.Value(model.Dictionary);

        writer.Name("clip");
        writer.Value(model.Clip);

        writer.Name("name");
        writer.Value(model.Name);

        writer.Name("flags");
        writer.Value((int)model.Flags);

        writer.EndObject();
    }
}