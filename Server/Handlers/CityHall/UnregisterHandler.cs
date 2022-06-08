using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;

namespace Server.Handlers.CityHall;

public class UnregisterHandler : ISingletonScript
{
    private readonly RegistrationOfficeService _registrationOfficeService;

    public UnregisterHandler(RegistrationOfficeService registrationOfficeService)
    {
        _registrationOfficeService = registrationOfficeService;

        AltAsync.OnClient<ServerPlayer>("cityhall:unregister", OnExecute);
    }

    private async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var officeEntryModel = await _registrationOfficeService.GetByKey(player.CharacterModel.Id);
        if (officeEntryModel != null)
        {
            await _registrationOfficeService.Remove(officeEntryModel);
            player.SendNotification("Charakter wurde aus dem Melderegister ausgetragen.", NotificationType.INFO);
        }
    }
}