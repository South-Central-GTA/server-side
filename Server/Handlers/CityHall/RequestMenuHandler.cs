using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;

namespace Server.Handlers.CityHall;

public class RequestMenuHandler : ISingletonScript
{
    private readonly RegistrationOfficeService _registrationOfficeService;
    
    public RequestMenuHandler(RegistrationOfficeService registrationOfficeService)
    {
        _registrationOfficeService = registrationOfficeService;
        
        AltAsync.OnClient<ServerPlayer>("cityhall:requestmenu", OnExecute);
    }

    private async void OnExecute(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var isRegistered = await _registrationOfficeService.IsRegistered(player.CharacterModel.Id);
        if (isRegistered)
        {
            player.CreateDialog(new DialogData
            {
                Type = DialogType.ONE_BUTTON_DIALOG,
                Title = "Stadthalle",
                Description = "Möchtest du deinen Charakter aus dem Melderegister austragen?" + 
                      "<br>In unserem Staat ist es IC legal nicht gemeldet zu sein." +
                      "<br><br><span class='text-muted'>Dein Charakter könnte dann folgende Dinge nicht mehr tun:<br>" +
                      "<ul><li>Bankkonten erstellen</li>" +
                      "<li>im Besitz befindene Bankkonten werden eingefroren</li>" +
                      "<li>Unternehmen gründen</li>" +
                      "<li>im Besitz befindene Unternehmen werden eingefroren</li>" +
                      "<li>Immobilien erwerben</li>" +
                      "<li>öffentliche Services nutzen (Garagen)</li>" +
                      "</ul></span>",
                FreezeGameControls = true,
                PrimaryButton = "Austragen",
                PrimaryButtonServerEvent = "cityhall:unregister"
            });
        }
        else
        {
            player.CreateDialog(new DialogData
            {
                Type = DialogType.ONE_BUTTON_DIALOG,
                Title = "Stadthalle",
                Description = "Möchtest du deinen Charakter in das Melderegister eintragen?" + 
                              "<br>In unserem Staat ist es IC legal nicht gemeldet zu sein." + 
                              "<br><br><span class='text-muted'>Dein Charakter könnte dann folgende Dinge tun:<br>" +
                              "<ul><li>Bankkonten erstellen</li>" +
                              "<li>im Besitz befindene Bankkonten werden entfroren</li>" +
                              "<li>Unternehmen gründen</li>" +
                              "<li>im Besitz befindene Unternehmen werden entfroren</li>" +
                              "<li>Immobilien erwerben</li>" +
                              "<li>öffentliche Services nutzen (Garagen)</li>" +
                              "</ul></span>",
                FreezeGameControls = true,
                PrimaryButton = "Eintragen",
                PrimaryButtonServerEvent = "cityhall:register"
            });
        }
    }
}