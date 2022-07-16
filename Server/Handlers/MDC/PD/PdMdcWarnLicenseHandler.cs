using System;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.PD;

public class PdMdcWarnLicenseHandler : ISingletonScript
{
    private readonly CriminalRecordModule _criminalRecordModule;
    private readonly FactionGroupService _factionGroupService;
    private readonly PersonalLicenseService _personalLicenseService;

    private readonly PoliceMdcModule _policeMdcModule;

    public PdMdcWarnLicenseHandler(PersonalLicenseService personalLicenseService,
        FactionGroupService factionGroupService, PoliceMdcModule policeMdcModule,
        CriminalRecordModule criminalRecordModule)
    {
        _personalLicenseService = personalLicenseService;
        _factionGroupService = factionGroupService;

        _policeMdcModule = policeMdcModule;
        _criminalRecordModule = criminalRecordModule;

        AltAsync.OnClient<ServerPlayer, PersonalLicensesType, int>("policemdc:warnlicense", OnWarnLicense);
    }

    private async void OnWarnLicense(ServerPlayer player, PersonalLicensesType type, int id)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _factionGroupService.GetByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            return;
        }

        var personalLicense = await _personalLicenseService.GetByKey(id);
        if (personalLicense == null)
        {
            return;
        }

        personalLicense.Warnings++;

        if (personalLicense.Warnings >= 3)
        {
            await _personalLicenseService.Remove(personalLicense);
        }
        else
        {
            await _personalLicenseService.Update(personalLicense);
        }

        var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(personalLicense.CharacterModelId);
        if (targetPlayer != null)
        {
            targetPlayer.CharacterModel.Licenses =
                await _personalLicenseService.Where(l => l.CharacterModelId == personalLicense.CharacterModelId);
            targetPlayer.SendNotification(
                $"Deinem Charakter wurde eine Verwarnung {personalLicense.Warnings}/3 ausgesprochen für den {GetLicenseName(personalLicense.Type)}.",
                NotificationType.WARNING);
            if (personalLicense.Warnings >= 3)
            {
                targetPlayer.SendNotification(
                    $"Dein Charakter wurde der {GetLicenseName(personalLicense.Type)} von {player.CharacterModel.Name} entzogen.",
                    NotificationType.ERROR);
            }
        }

        if (personalLicense.Warnings < 3)
        {
            await _criminalRecordModule.Add(personalLicense.CharacterModelId, player.CharacterModel.Name,
                $"Hat eine Verwarnung {personalLicense.Warnings}/3 für den {GetLicenseName(personalLicense.Type)} erhalten.");
        }
        else
        {
            await _criminalRecordModule.Add(personalLicense.CharacterModelId, player.CharacterModel.Name,
                $"Der {GetLicenseName(personalLicense.Type)} wurde nach 3/3 Verwarnungen automatisch entzogen.");
        }

        await _policeMdcModule.OpenCharacterRecord(player, personalLicense.CharacterModelId.ToString());
    }

    private string GetLicenseName(PersonalLicensesType personalLicenseType)
    {
        switch (personalLicenseType)
        {
            case PersonalLicensesType.DRIVING:
                return "Führerschein";
            case PersonalLicensesType.BOATS:
                return "Bootsschein";
            case PersonalLicensesType.FLYING:
                return "Flugschein";
            case PersonalLicensesType.WEAPON:
                return "Waffenschein";
            default:
                throw new ArgumentOutOfRangeException(nameof(personalLicenseType), personalLicenseType, null);
        }
    }
}