using System;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.Base;

public class MdcOpenSearchEntityHandler : ISingletonScript
{
    private readonly GroupFactionService _groupFactionService;

    private readonly FireMdcModule _fireMdcModule;
    private readonly PoliceMdcModule _policeMdcModule;

    public MdcOpenSearchEntityHandler(
        GroupFactionService groupFactionService,
        FireMdcModule fireMdcModule,
        PoliceMdcModule policeMdcModule)
    {
        _groupFactionService = groupFactionService;

        _fireMdcModule = fireMdcModule;
        _policeMdcModule = policeMdcModule;

        AltAsync.OnClient<ServerPlayer, string, MdcSearchType>("mdc:openmdcsearchentity", OnOpenEntity);
    }

    private async void OnOpenEntity(ServerPlayer player, string id, MdcSearchType type)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is null)
        {
            return;
        }

        if (factionGroup.FactionType == FactionType.POLICE_DEPARTMENT)
        {
            switch (type)
            {
                case MdcSearchType.NAME:
                    await _policeMdcModule.OpenCharacterRecord(player, id);
                    break;
                case MdcSearchType.NUMBER:
                    await _policeMdcModule.OpenPhoneRecord(player, id);
                    break;
                case MdcSearchType.VEHICLE:
                    await _policeMdcModule.OpenVehicleRecord(player, id);
                    break;
                case MdcSearchType.BANK_ACCOUNT:
                    await _policeMdcModule.OpenBankAccountRecord(player, id);
                    break;
                case MdcSearchType.MAIL:
                    await _policeMdcModule.OpenMailAccountRecord(player, id);
                    break;
                case MdcSearchType.WEAPON:
                    await _policeMdcModule.OpenWeaponRecord(player, id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
        else if (factionGroup.FactionType == FactionType.FIRE_DEPARTMENT)
        {
            if (type == MdcSearchType.NAME)
            {
                await _fireMdcModule.OpenPatientRecords(player, int.Parse(id));
            }
        }
    }
}