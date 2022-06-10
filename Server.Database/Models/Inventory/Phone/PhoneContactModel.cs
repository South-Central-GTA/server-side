using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory.Phone;

public class PhoneContactModel : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int ItemPhoneModelId { get; set; }
    public ItemPhoneModel ItemPhoneModel { get; set; }

    public string PhoneNumber { get; set; }
    public string Name { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(PhoneContactModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("phoneNumber");
        writer.Value(model.PhoneNumber);

        writer.Name("name");
        writer.Value(model.Name);

        writer.EndObject();
    }
}