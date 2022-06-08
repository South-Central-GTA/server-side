using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using AltV.Net;
using Server.Database.Models._Base;
using Server.Database.Models.Group;

namespace Server.Database.Models.File;

public class DirectoryModel
    : ModelBase
{
    public DirectoryModel()
    {
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public int GroupModelId { get; set; }

    public GroupModel GroupModel { get; set; }

    public List<FileModel> Files { get; set; } = new();

    [MaxLength(50)] public string Title { get; set; }

    public int ReadGroupLevel { get; set; }
    public int WriteGroupLevel { get; set; }

    public string LastEditCharacterName { get; set; }

    public int CreatorCharacterId { get; set; }
    public string CreatorCharacterName { get; set; }
}