using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AltV.Net;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory.Phone;

public class PhoneContactModel
    : ModelBase, IWritable
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
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("phoneNumber");
        writer.Value(PhoneNumber);

        writer.Name("name");
        writer.Value(Name);

        writer.EndObject();
    }
}