using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.MDC;

namespace Server.Handlers.MDC.FD;

public class FdMdcDeleteAllergyEntryHandler : ISingletonScript
{
    private readonly AllergiesModule _allergiesModule;
    private readonly FireMdcModule _fireMdcModule;
    private readonly GroupFactionService _groupFactionService;

    public FdMdcDeleteAllergyEntryHandler(GroupFactionService groupFactionService, AllergiesModule allergiesModule,
        FireMdcModule fireMdcModule)
    {
        _groupFactionService = groupFactionService;

        _allergiesModule = allergiesModule;
        _fireMdcModule = fireMdcModule;

        AltAsync.OnClient<ServerPlayer, int>("firemdc:deleteallergy", OnDeleteAllergyEntry);
    }

    private async void OnDeleteAllergyEntry(ServerPlayer player, int entryId)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is not { FactionType: FactionType.FIRE_DEPARTMENT })
        {
            return;
        }

        var allergyModel = await _allergiesModule.GetById(entryId);
        if (allergyModel == null)
        {
            return;
        }

        await _allergiesModule.Remove(allergyModel);

        await _fireMdcModule.OpenPatientRecords(player, allergyModel.CharacterModelId);
    }
}