using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.Banking;

public class BankAccountCharacterAccessModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int BankAccountModelId { get; set; }

    public BankAccountModel BankAccountModel { get; set; }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public CharacterModel? CharacterModel { get; set; }

    public BankingPermission Permission { get; set; }

    public bool Owner { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(BankAccountCharacterAccessModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("permission");
        writer.Value((int)model.Permission);

        writer.Name("owner");
        writer.Value(model.Owner);

        writer.Name("name");
        writer.Value(model.CharacterModel.Name);

        writer.Name("characterId");
        writer.Value(model.CharacterModel.Id);

        writer.EndObject();
    }
}