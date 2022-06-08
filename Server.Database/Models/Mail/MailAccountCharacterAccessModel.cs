using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.Mail;

public class MailAccountCharacterAccessModel
    : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string MailAccountModelMailAddress { get; set; }

    public MailAccountModel MailAccountModel { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public CharacterModel CharacterModel { get; set; }

    public MailingPermission Permission { get; set; }

    public bool Owner { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("permission");
        writer.Value((int)Permission);

        writer.Name("owner");
        writer.Value(Owner);

        writer.Name("name");
        writer.Value(CharacterModel.Name);

        writer.Name("characterId");
        writer.Value(CharacterModel.Id);

        writer.EndObject();
    }
}