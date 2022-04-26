using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Models;

namespace Server.Handlers.CityHall;

public class RegisterHandler : ISingletonScript
{
    private readonly RegistrationOfficeService _registrationOfficeService;
    
    public RegisterHandler(RegistrationOfficeService registrationOfficeService)
    {
        _registrationOfficeService = registrationOfficeService;
        
        AltAsync.OnClient<ServerPlayer>("cityhall:register", OnExecute);
    }

    private async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (!isRegistered)
        {
            await _registrationOfficeService.Add(new RegistrationOfficeEntryModel()
            {
                CharacterModelId = player.CharacterModel.Id
            });
            
            player.SendNotification("Charakter wurde in das Melderegister eingetragen.", NotificationType.INFO);
        }
    }
}