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

public class CharacterModel
    : PositionRotationDimensionModelBase, IWritable
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
            itemsToAdd.Add(new ItemModel(ItemCatalogIds.DOLLAR, 0, null, null, startMoney, null, true, false, ItemState.NOT_EQUIPPED));
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

    [JsonPropertyName("faceFeatures")] 
    public FaceFeaturesModel FaceFeaturesModel { get; set; }
    
    [JsonPropertyName("appearances")] 
    public AppearancesModel AppearancesModel { get; set; }
    
    [JsonPropertyName("inventory")] 
    public InventoryModel InventoryModel { get; set; }
    
    [JsonPropertyName("tattoos")] 
    public TattoosModel TattoosModel { get; set; }
    
    [JsonPropertyName("job")] 
    public DefinedJobModel? JobModel { get; set; }
    
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

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("id");
        writer.Value(Id);

        writer.Name("accountId");
        writer.Value(AccountModelId);

        writer.Name("currentAccountName");
        writer.Value(AccountModel != null ? AccountModel.CurrentName : "NotSavedCharacter");

        #region FaceFeatures

        writer.Name("faceFeatures");

        writer.BeginObject();

        writer.Name("eyesSize");
        writer.Value(FaceFeaturesModel.EyesSize);

        writer.Name("lipsThickness");
        writer.Value(FaceFeaturesModel.LipsThickness);

        writer.Name("noseWidth");
        writer.Value(FaceFeaturesModel.NoseWidth);

        writer.Name("noseHeight");
        writer.Value(FaceFeaturesModel.NoseHeight);

        writer.Name("noseLength");
        writer.Value(FaceFeaturesModel.NoseLength);

        writer.Name("noseBridge");
        writer.Value(FaceFeaturesModel.NoseBridge);

        writer.Name("noseTip");
        writer.Value(FaceFeaturesModel.NoseTip);

        writer.Name("noseBridgeShift");
        writer.Value(FaceFeaturesModel.NoseBridgeShift);

        writer.Name("browHeight");
        writer.Value(FaceFeaturesModel.BrowHeight);

        writer.Name("browWidth");
        writer.Value(FaceFeaturesModel.BrowWidth);

        writer.Name("cheekboneHeight");
        writer.Value(FaceFeaturesModel.CheekboneHeight);

        writer.Name("cheekboneWidth");
        writer.Value(FaceFeaturesModel.CheekboneWidth);

        writer.Name("cheekWidth");
        writer.Value(FaceFeaturesModel.CheekWidth);

        writer.Name("jawWidth");
        writer.Value(FaceFeaturesModel.JawWidth);

        writer.Name("jawHeight");
        writer.Value(FaceFeaturesModel.JawHeight);

        writer.Name("chinLength");
        writer.Value(FaceFeaturesModel.ChinLength);

        writer.Name("chinPosition");
        writer.Value(FaceFeaturesModel.ChinPosition);

        writer.Name("chinWidth");
        writer.Value(FaceFeaturesModel.ChinWidth);

        writer.Name("chinShape");
        writer.Value(FaceFeaturesModel.ChinShape);

        writer.Name("neckWidth");
        writer.Value(FaceFeaturesModel.NeckWidth);

        writer.EndObject();

        #endregion

        #region Appearances

        writer.Name("appearances");

        writer.BeginObject();

        writer.Name("hair");
        writer.Value(AppearancesModel.Hair);

        writer.Name("primHairColor");
        writer.Value(AppearancesModel.PrimHairColor);

        writer.Name("secHairColor");
        writer.Value(AppearancesModel.SecHairColor);

        writer.Name("eyeColor");
        writer.Value(AppearancesModel.EyeColor);

        writer.Name("blemishesValue");
        writer.Value(AppearancesModel.BlemishesValue);

        writer.Name("blemishesOpacity");
        writer.Value(AppearancesModel.BlemishesOpacity);

        writer.Name("blemishesColor");
        writer.Value(AppearancesModel.BlemishesColor);

        writer.Name("facialhairValue");
        writer.Value(AppearancesModel.FacialhairValue);

        writer.Name("facialhairOpacity");
        writer.Value(AppearancesModel.FacialhairOpacity);

        writer.Name("facialhairColor");
        writer.Value(AppearancesModel.FacialhairColor);

        writer.Name("eyebrowsValue");
        writer.Value(AppearancesModel.EyebrowsValue);

        writer.Name("eyebrowsOpacity");
        writer.Value(AppearancesModel.EyebrowsOpacity);

        writer.Name("eyebrowsColor");
        writer.Value(AppearancesModel.EyebrowsColor);

        writer.Name("ageingValue");
        writer.Value(AppearancesModel.AgeingValue);

        writer.Name("ageingOpacity");
        writer.Value(AppearancesModel.AgeingOpacity);

        writer.Name("ageingColor");
        writer.Value(AppearancesModel.AgeingColor);

        writer.Name("makeupValue");
        writer.Value(AppearancesModel.MakeupValue);

        writer.Name("makeupOpacity");
        writer.Value(AppearancesModel.MakeupOpacity);

        writer.Name("makeupColor");
        writer.Value(AppearancesModel.MakeupColor);

        writer.Name("blushValue");
        writer.Value(AppearancesModel.BlushValue);

        writer.Name("blushOpacity");
        writer.Value(AppearancesModel.BlushOpacity);

        writer.Name("blushColor");
        writer.Value(AppearancesModel.BlushColor);

        writer.Name("complexionValue");
        writer.Value(AppearancesModel.ComplexionValue);

        writer.Name("complexionOpacity");
        writer.Value(AppearancesModel.ComplexionOpacity);

        writer.Name("complexionColor");
        writer.Value(AppearancesModel.ComplexionColor);

        writer.Name("sundamageValue");
        writer.Value(AppearancesModel.SundamageValue);

        writer.Name("sundamageOpacity");
        writer.Value(AppearancesModel.SundamageOpacity);

        writer.Name("sundamageColor");
        writer.Value(AppearancesModel.SundamageColor);

        writer.Name("lipstickValue");
        writer.Value(AppearancesModel.LipstickValue);

        writer.Name("lipstickOpacity");
        writer.Value(AppearancesModel.LipstickOpacity);

        writer.Name("lipstickColor");
        writer.Value(AppearancesModel.LipstickColor);

        writer.Name("frecklesValue");
        writer.Value(AppearancesModel.FrecklesValue);

        writer.Name("frecklesOpacity");
        writer.Value(AppearancesModel.FrecklesOpacity);

        writer.Name("frecklesColor");
        writer.Value(AppearancesModel.FrecklesColor);

        writer.Name("chesthairValue");
        writer.Value(AppearancesModel.ChesthairValue);

        writer.Name("chesthairOpacity");
        writer.Value(AppearancesModel.ChesthairOpacity);

        writer.Name("chesthairColor");
        writer.Value(AppearancesModel.ChesthairColor);

        writer.Name("bodyblemishesValue");
        writer.Value(AppearancesModel.BodyblemishesValue);

        writer.Name("bodyblemishesOpacity");
        writer.Value(AppearancesModel.BodyblemishesOpacity);

        writer.Name("bodyblemishesColor");
        writer.Value(AppearancesModel.BodyblemishesColor);

        writer.Name("addbodyblemihesValue");
        writer.Value(AppearancesModel.AddbodyblemihesValue);

        writer.Name("addbodyblemihesOpacity");
        writer.Value(AppearancesModel.AddbodyblemihesOpacity);

        writer.Name("addbodyblemihesColor");
        writer.Value(AppearancesModel.AddbodyblemihesColor);

        writer.EndObject();

        #endregion

        #region Inventory

        writer.Name("inventory");

        writer.BeginObject();

        writer.Name("id");
        writer.Value(InventoryModel.Id);

        writer.Name("inventoryType");
        writer.Value((int)InventoryModel.InventoryType);

        writer.Name("items");

        writer.BeginArray();

        foreach (var item in InventoryModel.Items)
        {
            writer.BeginObject();

            writer.Name("id");
            writer.Value(item.Id);

            writer.Name("catalogItemName");
            writer.Value(item.CatalogItemModelId.ToString());

            writer.Name("slot");
            writer.Value(item.Slot ?? -1);

            writer.Name("customData");
            writer.Value(item.CustomData);

            writer.Name("note");
            writer.Value(item.Note);

            writer.Name("amount");
            writer.Value(item.Amount);

            writer.Name("condition");
            writer.Value(item.Condition ?? -1);

            writer.Name("isBought");
            writer.Value(item.IsBought);

            writer.Name("itemState");
            writer.Value((int)item.ItemState);

            writer.EndObject();
        }

        writer.EndArray();

        writer.Name("maxWeight");
        writer.Value(InventoryModel.MaxWeight);

        writer.EndObject();

        #endregion

        #region Tattoos

        writer.Name("tattoos");

        writer.BeginObject();

        writer.Name("headCollection");
        writer.Value(TattoosModel.HeadCollection);

        writer.Name("headHash");
        writer.Value(TattoosModel.HeadHash);

        writer.Name("torsoCollection");
        writer.Value(TattoosModel.TorsoCollection);

        writer.Name("torsoHash");
        writer.Value(TattoosModel.TorsoHash);

        writer.Name("leftArmCollection");
        writer.Value(TattoosModel.LeftArmCollection);

        writer.Name("leftArmHash");
        writer.Value(TattoosModel.LeftArmHash);

        writer.Name("rightArmCollection");
        writer.Value(TattoosModel.RightArmCollection);

        writer.Name("rightArmHash");
        writer.Value(TattoosModel.RightArmHash);

        writer.Name("leftLegCollection");
        writer.Value(TattoosModel.LeftLegCollection);

        writer.Name("leftLegHash");
        writer.Value(TattoosModel.LeftLegHash);

        writer.Name("rightLegCollection");
        writer.Value(TattoosModel.RightLegCollection);

        writer.Name("rightLegHash");
        writer.Value(TattoosModel.RightLegHash);

        writer.EndObject();
        
        #endregion

        #region DefinedJob

        if (JobModel != null)
        {
            writer.Name("definedJob");

            writer.BeginObject();

            writer.Name("jobId");
            writer.Value(JobModel.JobId);

            writer.Name("bankAccountId");
            writer.Value(JobModel.BankAccountId);

            writer.EndObject();
        }

        #endregion

        writer.Name("onlineSince");
        writer.Value(JsonSerializer.Serialize(OnlineSince));

        writer.Name("lastUsage");
        writer.Value(JsonSerializer.Serialize(LastUsage));

        writer.Name("createdAt");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.Name("firstName");
        writer.Value(FirstName);

        writer.Name("lastName");
        writer.Value(LastName);

        writer.Name("name");
        writer.Value(Name);

        writer.Name("age");
        writer.Value(Age);

        writer.Name("origin");
        writer.Value(Origin);

        writer.Name("physique");
        writer.Value(Physique);

        writer.Name("story");
        writer.Value(Story);

        writer.Name("bodySize");
        writer.Value(BodySize);

        writer.Name("gender");
        writer.Value((int)Gender);

        writer.Name("mother");
        writer.Value(Mother);

        writer.Name("father");
        writer.Value(Father);

        writer.Name("similarity");
        writer.Value(Similarity);

        writer.Name("skinSimilarity");
        writer.Value(SkinSimilarity);

        writer.Name("characterState");
        writer.Value((int)CharacterState);

        writer.Name("torso");
        writer.Value(Torso);

        writer.Name("torsoTexture");
        writer.Value(TorsoTexture);

        writer.Name("licenses");

        writer.BeginArray();

        foreach (var license in Licenses)
        {
            writer.BeginObject();

            writer.Name("id");
            writer.Value(license.Id);

            writer.Name("type");
            writer.Value((int)license.Type);

            writer.Name("warnings");
            writer.Value(license.Warnings);

            writer.EndObject();
        }

        writer.EndArray();
        
        writer.EndObject();
    }
}