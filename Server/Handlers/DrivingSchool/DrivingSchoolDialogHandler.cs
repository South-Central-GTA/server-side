using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;

namespace Server.Handlers.DrivingSchool;

public class DrivingSchoolDialogHandler : ISingletonScript
{
    private readonly BankModule _bankModule;
    private readonly RegistrationOfficeService _registrationOfficeService;
    private readonly WorldLocationOptions _worldLocationOptions;

    public DrivingSchoolDialogHandler(IOptions<WorldLocationOptions> worldLocationOptions,
        RegistrationOfficeService registrationOfficeService, BankModule bankModule)
    {
        _worldLocationOptions = worldLocationOptions.Value;

        _registrationOfficeService = registrationOfficeService;

        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer>("drivingschool:showstartdialog", OnShowStartDialog);
    }

    private async void OnShowStartDialog(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        if (!await _bankModule.HasBankAccount(player))
        {
            player.SendNotification("Dein Charakter benötigt ein Bankkonto um eine Prüfung zu starten.",
                NotificationType.ERROR);
            return;
        }

        var drivingSchoolData = _worldLocationOptions.DrivingSchools.Find(g =>
            player.Position.Distance(new Position(g.PedPointX, g.PedPointY, g.PedPointZ)) <= 3);
        if (drivingSchoolData == null)
        {
            player.SendNotification("Es befindet sich keine Fahrschule in der Nähe deines Charakters.",
                NotificationType.ERROR);
            return;
        }

        if (player.CharacterModel.Licenses.Exists(l => l.Type == PersonalLicensesType.DRIVING))
        {
            player.SendNotification(
                "Dein Charakter hat schon einen Führerschein, falls dir das Lizenz Item fehlt beantrage es am Tresen im Police Department.",
                NotificationType.ERROR);
            return;
        }

        player.CreateDialog(new DialogData
        {
            Type = DialogType.ONE_BUTTON_DIALOG,
            Title = "Führerschein Prüfung",
            Description =
                $"Möchtest du die Prüfung für <b>${drivingSchoolData.DrivingLicensePrice}</b> beginnen?<br>" +
                "Du solltest dir davor die Straßenverkehrsordnung mit deinem Charakter auf der Internetseite des Governments durchlesen.<br><br>" +
                "<span class='text-muted'>Die Kosten werden von deinem angegebenen Bankkonto abgezogen.</span>",
            HasBankAccountSelection = true,
            FreezeGameControls = true,
            PrimaryButton = "Prüfung beginnen",
            PrimaryButtonServerEvent = "drivingschool:start"
        });
    }
}