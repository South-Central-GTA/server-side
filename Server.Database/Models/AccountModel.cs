using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Character;
using Server.Database.Models.CustomLogs;

namespace Server.Database.Models;

public class AccountModel
    : ModelBase, IWritable
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public ulong SocialClubId { get; set; }
    
    [NotMapped]
    public string? AvatarUrl { get; set; }

    public ulong DiscordId { get; set; }
    public string PasswordHash { get; set; }
    public string CurrentName { get; set; }
    public List<string> NameHistory { get; init; } = new();

    public List<CharacterModel> Characters { get; init; } = new();
    public List<CommandLogModel> CommandLogs { get; init; } = new();
    public List<ChatLogModel> ChatLogs { get; init; } = new(); 
    public List<UserRecordLogModel> UserRecords { get; init; } = new();

    public ulong? HardwareIdHash { get; set; }
    public ulong? HardwareIdExHash { get; set; }
    public Permission Permission { get; set; }
    public ulong BannedFrom { get; set; }
    public string? BannedReason { get; set; }
    public bool BannedPermanent { get; set; }

    [JsonIgnore] public DateTime BannedUntil { get; set; }

    [JsonIgnore] public DateTime? LastLogin { get; set; }

    [JsonIgnore] public DateTime OnlineSince { get; set; }

    public int AdminCheckpoints { get; set; }
    public int SouthCentralPoints { get; set; }
    public string LastIp { get; set; }
    public int LastSelectedCharacterId { get; set; }

    public int MaxCharacters { get; set; }
    public int MaxAnimations { get; set; }
    public int MaxRoleplayInfos { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(SocialClubId);

        writer.Name("discordId");
        writer.Value(DiscordId.ToString());

        writer.Name("currentName");
        writer.Value(CurrentName);

        writer.Name("nameHistoryJson");
        writer.Value(JsonSerializer.Serialize(NameHistory));

        writer.Name("southCentralPoints");
        writer.Value(SouthCentralPoints);

        writer.Name("avatarUrl");
        writer.Value(AvatarUrl);

        writer.Name("lastUsage");
        writer.Value(JsonSerializer.Serialize(LastUsage));

        writer.Name("createdAt");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.EndObject();
    }
}