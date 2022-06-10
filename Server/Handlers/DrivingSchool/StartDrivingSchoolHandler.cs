using System.Linq;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;
using Server.Modules.DrivingSchool;

namespace Server.Handlers.DrivingSchool;

public class StartDrivingSchoolHandler : ISingletonScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;

    private readonly DrivingSchoolModule _drivingSchoolModule;
    private readonly WorldLocationOptions _worldLocationOptions;

    public StartDrivingSchoolHandler(IOptions<WorldLocationOptions> worldLocationOptions,
        BankAccountService bankAccountService, DrivingSchoolModule drivingSchoolModule, BankModule bankModule)
    {
        _worldLocationOptions = worldLocationOptions.Value;

        _bankAccountService = bankAccountService;

        _drivingSchoolModule = drivingSchoolModule;
        _bankModule = bankModule;

        AltAsync.OnClient<ServerPlayer, int>("drivingschool:start", OnStart);
    }

    private async void OnStart(ServerPlayer player, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }

        var drivingSchoolData = _worldLocationOptions.DrivingSchools.Find(g =>
            player.Position.Distance(new Position(g.PedPointX, g.PedPointY, g.PedPointZ)) <= 3);
        if (drivingSchoolData == null)
        {
            player.SendNotification(
                "Es befindet sich keine Fahrschule in der Nähe deines Charakters die Prüfung konnte nicht begonnen werden.",
                NotificationType.ERROR);
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (Alt.GetAllVehicles().FirstOrDefault(v => v.Position.Distance(new Position(drivingSchoolData.StartPointX,
                drivingSchoolData.StartPointY, drivingSchoolData.StartPointZ)) <= 2) != null || Alt.GetAllPlayers()
                .FirstOrDefault(p => p.Position.Distance(new Position(drivingSchoolData.StartPointX,
                    drivingSchoolData.StartPointY, drivingSchoolData.StartPointZ)) <= 2) != null)
        {
            player.SendNotification("Die Prüfung konnte nicht begonnen werden, da der Parkplatz besetzt ist.",
                NotificationType.ERROR);
            return;
        }

        if (_drivingSchoolModule.ExistExamVehicleForPlayer(player, out var vehicle))
        {
            player.SendNotification("Du bist bereits in einer Prüfung.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.TRANSFER))
        {
            player.SendNotification($"Dein Charakter hat keine Transferrechte für das Konto {bankAccount.BankDetails}.",
                NotificationType.ERROR);
            return;
        }

        var success = await _bankModule.Withdraw(bankAccount, drivingSchoolData.DrivingLicensePrice, false,
            "Führerscheinprüfung");
        if (success)
        {
            await _drivingSchoolModule.SetPlayerInExam(drivingSchoolData, player);
        }
        else
        {
            player.SendNotification("Dein Charakter hat nicht genug Geld auf dem Bankkonto für die Prüfung.",
                NotificationType.ERROR);
        }
    }
}