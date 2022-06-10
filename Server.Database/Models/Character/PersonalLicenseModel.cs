using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Character;

public class PersonalLicenseModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int CharacterModelId { get; set; }

    public CharacterModel CharacterModel { get; set; }


    public PersonalLicensesType Type { get; set; }
    public int Warnings { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(PersonalLicenseModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("type");
        writer.Value((int)model.Type);

        writer.Name("warnings");
        writer.Value(model.Warnings);

        writer.EndObject();
    }
}