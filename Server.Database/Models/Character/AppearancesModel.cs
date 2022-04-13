using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Server.Database.Models._Base;

namespace Server.Database.Models.Character;

public class AppearancesModel
    : ModelBase
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int CharacterModelId { get; set; }

    public CharacterModel CharacterModel { get; set; }

    [JsonPropertyName("hair")] public int Hair { get; set; }

    [JsonPropertyName("primHaircolor")] public int PrimHairColor { get; set; }

    [JsonPropertyName("secHaircolor")] public int SecHairColor { get; set; }

    [JsonPropertyName("eyeColor")] public int EyeColor { get; set; }

    [JsonPropertyName("blemishesValue")] public int BlemishesValue { get; set; }

    [JsonPropertyName("blemishesOpacity")] public float BlemishesOpacity { get; set; }

    [JsonPropertyName("blemishesColor")] public int BlemishesColor { get; set; }

    [JsonPropertyName("facialhairValue")] public int FacialhairValue { get; set; }

    [JsonPropertyName("facialhairOpacity")]
    public float FacialhairOpacity { get; set; }

    [JsonPropertyName("facialhairColor")] public int FacialhairColor { get; set; }

    [JsonPropertyName("eyebrowsValue")] public int EyebrowsValue { get; set; }

    [JsonPropertyName("eyebrowsOpacity")] public float EyebrowsOpacity { get; set; }

    [JsonPropertyName("eyebrowsColor")] public int EyebrowsColor { get; set; }

    [JsonPropertyName("ageingValue")] public int AgeingValue { get; set; }

    [JsonPropertyName("ageingOpacity")] public float AgeingOpacity { get; set; }

    [JsonPropertyName("ageingColor")] public int AgeingColor { get; set; }

    [JsonPropertyName("makeupValue")] public int MakeupValue { get; set; }

    [JsonPropertyName("makeupOpacity")] public float MakeupOpacity { get; set; }

    [JsonPropertyName("makeupColor")] public int MakeupColor { get; set; }

    [JsonPropertyName("blushValue")] public int BlushValue { get; set; }

    [JsonPropertyName("blushOpacity")] public float BlushOpacity { get; set; }

    [JsonPropertyName("blushColor")] public int BlushColor { get; set; }

    [JsonPropertyName("complexionValue")] public int ComplexionValue { get; set; }

    [JsonPropertyName("complexionOpacity")]
    public float ComplexionOpacity { get; set; }

    [JsonPropertyName("complexionColor")] public int ComplexionColor { get; set; }

    [JsonPropertyName("sundamageValue")] public int SundamageValue { get; set; }

    [JsonPropertyName("sundamageOpacity")] public float SundamageOpacity { get; set; }

    [JsonPropertyName("sundamageColor")] public int SundamageColor { get; set; }

    [JsonPropertyName("lipstickValue")] public int LipstickValue { get; set; }

    [JsonPropertyName("lipstickOpacity")] public float LipstickOpacity { get; set; }

    [JsonPropertyName("lipstickColor")] public int LipstickColor { get; set; }

    [JsonPropertyName("frecklesValue")] public int FrecklesValue { get; set; }

    [JsonPropertyName("frecklesOpacity")] public float FrecklesOpacity { get; set; }

    [JsonPropertyName("frecklesColor")] public int FrecklesColor { get; set; }

    [JsonPropertyName("chesthairValue")] public int ChesthairValue { get; set; }

    [JsonPropertyName("chesthairOpacity")] public float ChesthairOpacity { get; set; }

    [JsonPropertyName("chesthairColor")] public int ChesthairColor { get; set; }

    [JsonPropertyName("bodyblemishesValue")]
    public int BodyblemishesValue { get; set; }

    [JsonPropertyName("bodyblemishesOpacity")]
    public float BodyblemishesOpacity { get; set; }

    [JsonPropertyName("bodyblemishesColor")]
    public int BodyblemishesColor { get; set; }

    [JsonPropertyName("addbodyblemihesValue")]
    public int AddbodyblemihesValue { get; set; }

    [JsonPropertyName("addbodyblemihesOpacity")]
    public float AddbodyblemihesOpacity { get; set; }

    [JsonPropertyName("addbodyblemihesColor")]
    public int AddbodyblemihesColor { get; set; }

    public void Update(AppearancesModel appearancesModel)
    {
        Hair = appearancesModel.Hair;
        PrimHairColor = appearancesModel.PrimHairColor;
        SecHairColor = appearancesModel.SecHairColor;

        EyeColor = appearancesModel.EyeColor;

        BlemishesValue = appearancesModel.BlemishesValue;
        BlemishesOpacity = appearancesModel.BlemishesOpacity;
        BlemishesColor = appearancesModel.BlemishesColor;

        FacialhairValue = appearancesModel.FacialhairValue;
        FacialhairOpacity = appearancesModel.FacialhairOpacity;
        FacialhairColor = appearancesModel.FacialhairColor;

        EyebrowsValue = appearancesModel.EyebrowsValue;
        EyebrowsOpacity = appearancesModel.EyebrowsOpacity;
        EyebrowsColor = appearancesModel.EyebrowsColor;

        AgeingValue = appearancesModel.AgeingValue;
        AgeingOpacity = appearancesModel.AgeingOpacity;
        AgeingColor = appearancesModel.AgeingColor;

        MakeupValue = appearancesModel.MakeupValue;
        MakeupOpacity = appearancesModel.MakeupOpacity;
        MakeupColor = appearancesModel.MakeupColor;

        BlushValue = appearancesModel.BlushValue;
        BlushOpacity = appearancesModel.BlushOpacity;
        BlushColor = appearancesModel.BlushColor;

        ComplexionValue = appearancesModel.ComplexionValue;
        ComplexionOpacity = appearancesModel.ComplexionOpacity;
        ComplexionColor = appearancesModel.ComplexionColor;

        SundamageValue = appearancesModel.SundamageValue;
        SundamageOpacity = appearancesModel.SundamageOpacity;
        SundamageColor = appearancesModel.SundamageColor;

        LipstickValue = appearancesModel.LipstickValue;
        LipstickOpacity = appearancesModel.LipstickOpacity;
        LipstickColor = appearancesModel.LipstickColor;

        FrecklesValue = appearancesModel.FrecklesValue;
        FrecklesOpacity = appearancesModel.FrecklesOpacity;
        FrecklesColor = appearancesModel.FrecklesColor;

        ChesthairValue = appearancesModel.ChesthairValue;
        ChesthairOpacity = appearancesModel.ChesthairOpacity;
        ChesthairColor = appearancesModel.ChesthairColor;

        BodyblemishesValue = appearancesModel.BodyblemishesValue;
        BodyblemishesOpacity = appearancesModel.BodyblemishesOpacity;
        BodyblemishesColor = appearancesModel.BodyblemishesColor;

        AddbodyblemihesValue = appearancesModel.AddbodyblemihesValue;
        AddbodyblemihesOpacity = appearancesModel.AddbodyblemihesOpacity;
        AddbodyblemihesColor = appearancesModel.AddbodyblemihesColor;
    }

    public int Diff(AppearancesModel appearancesModel)
    {
        var diffs = 0;

        if (Hair != appearancesModel.Hair) 
        {
            diffs ++;
        }
        
        if (PrimHairColor != appearancesModel.PrimHairColor) 
        {
            diffs ++;
        }
        
        if (SecHairColor != appearancesModel.SecHairColor) 
        {
            diffs ++;
        }
        
        if (EyeColor != appearancesModel.EyeColor) 
        {
            diffs ++;
        }
        
        if (BlemishesValue != appearancesModel.BlemishesValue) 
        {
            diffs ++;
        }
        
        if (BlemishesOpacity != appearancesModel.BlemishesOpacity) 
        {
            diffs ++;
        }
        
        if (BlemishesColor != appearancesModel.BlemishesColor) 
        {
            diffs ++;
        }
        
        if (FacialhairValue != appearancesModel.FacialhairValue) 
        {
            diffs ++;
        }
        
        if (FacialhairOpacity != appearancesModel.FacialhairOpacity) 
        {
            diffs ++;
        }
        
        if (FacialhairColor != appearancesModel.FacialhairColor) 
        {
            diffs ++;
        }
        
        if (EyebrowsValue != appearancesModel.EyebrowsValue) 
        {
            diffs ++;
        }
        
        if (EyebrowsOpacity != appearancesModel.EyebrowsOpacity) 
        {
            diffs ++;
        }
        
        if (EyebrowsColor != appearancesModel.EyebrowsColor) 
        {
            diffs ++;
        }
        
        if (AgeingValue != appearancesModel.AgeingValue) 
        {
            diffs ++;
        }
        
        if (AgeingOpacity != appearancesModel.AgeingOpacity) 
        {
            diffs ++;
        }
        
        if (AgeingColor != appearancesModel.AgeingColor) 
        {
            diffs ++;
        }
        
        if (MakeupValue != appearancesModel.MakeupValue) 
        {
            diffs ++;
        }
        
        if (MakeupOpacity != appearancesModel.MakeupOpacity) 
        {
            diffs ++;
        }
        
        if (MakeupColor != appearancesModel.MakeupColor) 
        {
            diffs ++;
        }
        
        if (BlushValue != appearancesModel.BlushValue) 
        {
            diffs ++;
        }
        
        if (BlushOpacity != appearancesModel.BlushOpacity) 
        {
            diffs ++;
        }
        
        if (BlushColor != appearancesModel.BlushColor) 
        {
            diffs ++;
        }
        
        if (ComplexionValue != appearancesModel.ComplexionValue) 
        {
            diffs ++;
        }
        
        if (ComplexionOpacity != appearancesModel.ComplexionOpacity) 
        {
            diffs ++;
        }
        
        if (ComplexionColor != appearancesModel.ComplexionColor) 
        {
            diffs ++;
        }
        
        if (SundamageValue != appearancesModel.SundamageValue) 
        {
            diffs ++;
        }
        
        if (SundamageOpacity != appearancesModel.SundamageOpacity) 
        {
            diffs ++;
        }
        
        if (SundamageColor != appearancesModel.SundamageColor) 
        {
            diffs ++;
        }
        
        if (LipstickValue != appearancesModel.LipstickValue) 
        {
            diffs ++;
        }
        
        if (LipstickOpacity != appearancesModel.LipstickOpacity) 
        {
            diffs ++;
        }
        
        if (LipstickColor != appearancesModel.LipstickColor) 
        {
            diffs ++;
        }
        
        if (FrecklesValue != appearancesModel.FrecklesValue) 
        {
            diffs ++;
        }
        
        if (FrecklesOpacity != appearancesModel.FrecklesOpacity) 
        {
            diffs ++;
        }
        
        if (FrecklesColor != appearancesModel.FrecklesColor) 
        {
            diffs ++;
        }
        
        if (ChesthairValue != appearancesModel.ChesthairValue) 
        {
            diffs ++;
        }
        
        if (ChesthairOpacity != appearancesModel.ChesthairOpacity) 
        {
            diffs ++;
        }
        
        if (ChesthairColor != appearancesModel.ChesthairColor) 
        {
            diffs ++;
        }
        
        if (BodyblemishesValue != appearancesModel.BodyblemishesValue) 
        {
            diffs ++;
        }
        
        if (BodyblemishesOpacity != appearancesModel.BodyblemishesOpacity) 
        {
            diffs ++;
        }
        
        if (BodyblemishesColor != appearancesModel.BodyblemishesColor) 
        {
            diffs ++;
        }
        
        if (AddbodyblemihesValue != appearancesModel.AddbodyblemihesValue) 
        {
            diffs ++;
        }
        
        if (AddbodyblemihesOpacity != appearancesModel.AddbodyblemihesOpacity) 
        {
            diffs ++;
        }
        
        if (AddbodyblemihesColor != appearancesModel.AddbodyblemihesColor) 
        {
            diffs ++;
        }
        
        
        return diffs;
    }
}