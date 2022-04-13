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

public class PdMdcRemoveLicenseHandler : ISingletonScript
{
    private readonly PersonalLicenseService _personalLicenseService;
    private readonly GroupFactionService _groupFactionService;

    private readonly PoliceMdcModule _policeMdcModule;
    private readonly CriminalRecordModule _criminalRecordModule;

    public PdMdcRemoveLicenseHandler(
        PersonalLicenseService personalLicenseService,
        GroupFactionService groupFactionService,
        
        PoliceMdcModule policeMdcModule, 
        CriminalRecordModule criminalRecordModule)
    {
        _personalLicenseService = personalLicenseService;
        _groupFactionService = groupFactionService;
        
        _policeMdcModule = policeMdcModule;
        _criminalRecordModule = criminalRecordModule;

        AltAsync.OnClient<ServerPlayer, PersonalLicensesType, int>("policemdc:removelicense", OnRemoveLicense);
    }

    private async void OnRemoveLicense(ServerPlayer player, PersonalLicensesType type, int id)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.POLICE_DEPARTMENT })
        {
            return;
        }
        
        var personalLicense = await _personalLicenseService.GetByKey(id);
        if (personalLicense == null)
        {
            return;
        }

        await _personalLicenseService.Remove(personalLicense);

        var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(personalLicense.CharacterModelId);
        if (targetPlayer != null)
        {
            targetPlayer.CharacterModel.Licenses =
                await _personalLicenseService.Where(l => l.CharacterModelId == personalLicense.CharacterModelId);
            targetPlayer.SendNotification(
                $"Der {GetLicenseName(personalLicense.Type)} wurde deinem Charakter von {player.CharacterModel.Name} entzogen.",
                NotificationType.ERROR);
        }

        await _criminalRecordModule.Add(personalLicense.CharacterModelId,
                                        player.CharacterModel.Name,
                                        $"Der {GetLicenseName(personalLicense.Type)} wurde entzogen.");

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