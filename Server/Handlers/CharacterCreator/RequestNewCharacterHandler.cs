using System;
using System.Collections.Generic;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Database.Models.Character;
using Server.Database.Models.Inventory;
using Server.Modules.Character;

namespace Server.Handlers.CharacterCreator;

public class RequestNewCharacterHandler : ISingletonScript
{
    private readonly CharacterCreationModule _characterCreationModule;
    
    public RequestNewCharacterHandler(CharacterCreationModule characterCreationModule)
    {
        _characterCreationModule = characterCreationModule;
        
        AltAsync.OnClient<ServerPlayer>("charcreator:requestnewchar", OnRequestNewChar);
    }

    private async void OnRequestNewChar(ServerPlayer player)
    {
        await _characterCreationModule.OpenAsync(player,
            new CharacterModel
            {
                FirstName = string.Empty,
                LastName = string.Empty,
                Origin = string.Empty,
                Physique = string.Empty,
                Story = string.Empty,
                OnlineSince = DateTime.Now,
                CreatedAt = DateTime.Now,
                LastUsage = DateTime.Now,

                // Set default parents
                Father = 0,
                Mother = 21,
                Similarity = 0.5f,
                SkinSimilarity = 0.5f,

                // Set complete torso as default torso because in character creation we are "naked".
                Torso = 15,
                TorsoTexture = 0,
                FaceFeaturesModel = new FaceFeaturesModel(),
                AppearancesModel = new AppearancesModel
                {
                    BlemishesValue = 255,
                    BlemishesOpacity = 1,
                    FacialhairValue = 255,
                    FacialhairOpacity = 1,
                    EyebrowsValue = 255,
                    EyebrowsOpacity = 1,
                    AgeingValue = 255,
                    AgeingOpacity = 1,
                    MakeupValue = 255,
                    MakeupOpacity = 1,
                    BlushValue = 255,
                    BlushOpacity = 1,
                    ComplexionValue = 255,
                    ComplexionOpacity = 1,
                    SundamageValue = 255,
                    SundamageOpacity = 1,
                    LipstickValue = 255,
                    LipstickOpacity = 255,
                    FrecklesValue = 255,
                    FrecklesOpacity = 1,
                    ChesthairValue = 255,
                    ChesthairOpacity = 1,
                    BodyblemishesValue = 255,
                    BodyblemishesOpacity = 1,
                    AddbodyblemihesValue = 255,
                    AddbodyblemihesOpacity = 1
                },
                InventoryModel = new InventoryModel { Items = new List<ItemModel>() },
                TattoosModel = new TattoosModel()
            });
    }
}