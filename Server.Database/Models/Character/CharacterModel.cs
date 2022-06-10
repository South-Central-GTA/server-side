using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;
using Server.Database.Models.Inventory;

namespace Server.Database.Models.Character;

public class CharacterModel : PositionRotationDimensionModelBase, IWritable
{
    public CharacterModel()
    {
    }

    public CharacterModel(CharacterModel characterModel, int startMoney)
    {
        AccountModelId = characterModel.AccountModelId;
        AccountModel = characterModel.AccountModel;
        OnlineSince = DateTime.Now;
        CreatedAt = DateTime.Now;
        LastUsage = DateTime.Now;

        FirstName = characterModel.FirstName;
        LastName = characterModel.LastName;

        Age = characterModel.Age;
        Origin = characterModel.Origin;
        Physique = characterModel.Physique;
        Story = characterModel.Story;
        BodySize = characterModel.BodySize;
        Gender = characterModel.Gender;

        Mother = characterModel.Mother;
        Father = characterModel.Father;
        Similarity = characterModel.Similarity;
        SkinSimilarity = characterModel.SkinSimilarity;
        CharacterState = CharacterState.PLAYABLE;

        Torso = characterModel.Torso;
        TorsoTexture = characterModel.TorsoTexture;

        AppearancesModel = characterModel.AppearancesModel;
        FaceFeaturesModel = characterModel.FaceFeaturesModel;
        TattoosModel = characterModel.TattoosModel;

        Health = 200;
        Armor = 0;
        Dimension = 0;
        DeathState = DeathState.ALIVE;

        var itemsToAdd = new List<ItemModel>();

        if (startMoney > 0)
        {
            itemsToAdd.Add(new ItemModel(ItemCatalogIds.DOLLAR, 0, null, null, startMoney, null, true, false,
                ItemState.NOT_EQUIPPED));
        }

        InventoryModel = new InventoryModel
        {
            InventoryType = InventoryType.PLAYER,
            MaxWeight = 12,
            Items = itemsToAdd,
            Name = characterModel.Name + "'s Taschen"
        };

        PositionX = characterModel.PositionX;
        PositionY = characterModel.PositionY;
        PositionZ = characterModel.PositionZ;

        Pitch = characterModel.Pitch;
        Roll = characterModel.Roll;
        Yaw = characterModel.Yaw;
    }

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; init; }

    public ulong AccountModelId { get; set; }
    public AccountModel? AccountModel { get; set; }
    public RegistrationOfficeEntryModel? RegistrationOfficeEntryModel { get; set; }

    [JsonPropertyName("faceFeatures")] public FaceFeaturesModel FaceFeaturesModel { get; set; }

    [JsonPropertyName("appearances")] public AppearancesModel AppearancesModel { get; set; }

    [JsonPropertyName("inventory")] public InventoryModel InventoryModel { get; set; }

    [JsonPropertyName("tattoos")] public TattoosModel TattoosModel { get; set; }

    [JsonPropertyName("job")] public DefinedJobModel? JobModel { get; set; }

    [JsonIgnore] public DateTime OnlineSince { get; set; }

    public string FirstName { get; set; }
    public string LastName { get; set; }

    [NotMapped] public string Name => $"{FirstName} {LastName}";

    public int Age { get; set; }
    public string Origin { set; get; }

    [MaxLength(512)] public string Physique { set; get; }

    [MaxLength(2048)] public string Story { set; get; }

    public int BodySize { set; get; }
    public GenderType Gender { set; get; }
    public int Mother { get; set; }
    public int Father { get; set; }
    public float Similarity { get; set; }
    public float SkinSimilarity { get; set; }
    public CharacterState CharacterState { get; set; }
    public int Torso { get; set; }
    public int TorsoTexture { get; set; }

    public ushort Health { set; get; }
    public ushort Armor { set; get; }

    public DeathState DeathState { set; get; }

    public List<int> AnimationIds { get; set; } = new();
    public List<PersonalLicenseModel> Licenses { get; set; } = new();

    public DateTime? JailedUntil { get; set; }
    public string? JailedByCharacterName { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(CharacterModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(model.Id);

        writer.Name("accountId");
        writer.Value(model.AccountModelId);

        writer.Name("accountName");
        writer.Value(model.AccountModel != null ? model.AccountModel.CurrentName : "NotSavedCharacter");

        #region Inventory

        writer.Name("inventory");

        InventoryModel.Serialize(model.InventoryModel, writer);

        #endregion

        #region FaceFeatures

        writer.Name("faceFeatures");

        FaceFeaturesModel.Serialize(model.FaceFeaturesModel, writer);

        #endregion

        #region Appearances

        writer.Name("appearances");

        AppearancesModel.Serialize(model.AppearancesModel, writer);

        #endregion

        #region Tattoos

        writer.Name("tattoos");

        TattoosModel.Serialize(model.TattoosModel, writer);

        #endregion

        writer.Name("onlineSinceJson");
        writer.Value(JsonSerializer.Serialize(model.OnlineSince));

        writer.Name("lastUsageJson");
        writer.Value(JsonSerializer.Serialize(model.LastUsage));

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(model.CreatedAt));

        writer.Name("firstName");
        writer.Value(model.FirstName);

        writer.Name("lastName");
        writer.Value(model.LastName);

        writer.Name("name");
        writer.Value(model.Name);

        writer.Name("age");
        writer.Value(model.Age);

        writer.Name("origin");
        writer.Value(model.Origin);

        writer.Name("physique");
        writer.Value(model.Physique);

        writer.Name("story");
        writer.Value(model.Story);

        writer.Name("bodySize");
        writer.Value(model.BodySize);

        writer.Name("gender");
        writer.Value((int)model.Gender);

        writer.Name("mother");
        writer.Value(model.Mother);

        writer.Name("father");
        writer.Value(model.Father);

        writer.Name("similarity");
        writer.Value(model.Similarity);

        writer.Name("skinSimilarity");
        writer.Value(model.SkinSimilarity);

        writer.Name("characterState");
        writer.Value((int)model.CharacterState);

        writer.Name("torso");
        writer.Value(model.Torso);

        writer.Name("torsoTexture");
        writer.Value(model.TorsoTexture);

        writer.Name("licenses");

        writer.BeginArray();

        foreach (var license in model.Licenses)
        {
            PersonalLicenseModel.Serialize(license, writer);
        }

        writer.EndArray();

        writer.EndObject();
    }
}