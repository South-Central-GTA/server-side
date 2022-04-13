using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Inventory.Phone;

public class PhoneNotificationModel
    : ModelBase
{
    public PhoneNotificationModel()
    {
    }

    public PhoneNotificationModel(int itemPhoneModelId, PhoneNotificationType type, string context)
    {
        ItemPhoneModelId = itemPhoneModelId;
        Type = type;
        Context = context;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int ItemPhoneModelId { get; set; }
    public ItemPhoneModel ItemPhoneModel { get; set; }

    public string Context { get; set; }
    public PhoneNotificationType Type { get; set; }
}