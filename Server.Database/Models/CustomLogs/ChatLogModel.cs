using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;

namespace Server.Database.Models.CustomLogs;

public class ChatLogModel
: ModelBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public ulong AccountModelId { get; set; }
    public AccountModel? AccountModel { get; set; }

    public int CharacterModelId { get; set; }
    public CharacterModel? CharacterModel { get; set; }

    public ChatType ChatType { get; set; }

    [MaxLength(2048)] public string Text { get; set; }

    public DateTime LoggedAt { get; set; }
}