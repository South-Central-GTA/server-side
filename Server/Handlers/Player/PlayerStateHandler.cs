using System.Linq;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Housing;
using Server.Modules.EntitySync;
using Server.Modules.Houses;

namespace Server.Handlers.Player;

public class PlayerStateHandler : ISingletonScript
{
    private readonly GroupService _groupService;

    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;
    private readonly PedSyncModule _pedSyncModule;

    public PlayerStateHandler(HouseService houseService, GroupService groupService, HouseModule houseModule,
        PedSyncModule pedSyncModule)
    {
        _houseService = houseService;
        _groupService = groupService;

        _houseModule = houseModule;
        _pedSyncModule = pedSyncModule;

        AltAsync.OnClient<ServerPlayer, bool>("player:setphonestate", OnSetPhoneState);
        AltAsync.OnClient<ServerPlayer, bool>("player:setinventorystate", OnSetInventoryState);
        AltAsync.OnClient<ServerPlayer, int>("player:clearduty", OnClearDuty);
    }

    private void OnSetPhoneState(ServerPlayer player, bool state)
    {
        player.IsPhoneOpen = state;
    }

    private void OnSetInventoryState(ServerPlayer player, bool state)
    {
        player.IsInventoryOpen = state;
    }

    private async void OnClearDuty(ServerPlayer player, int leaseCompanyHouseId)
    {
        if (!player.Exists)
        {
            return;
        }

        lock (player)
        {
            player.IsDuty = false;
            player.DeleteStreamSyncedMetaData("DUTY");
        }

        if (await _houseService.GetByKey(leaseCompanyHouseId) is not LeaseCompanyHouseModel leaseCompanyHouse)
        {
            return;
        }

        var houseOwningGroup = await _groupService.GetByKey(leaseCompanyHouse.GroupModelId);
        if (houseOwningGroup == null)
        {
            return;
        }

        var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
        if (groups == null || groups.Count == 0)
        {
            return;
        }

        if (groups.All(g => g.Id != houseOwningGroup.Id))
        {
            return;
        }

        leaseCompanyHouse.PlayerDuties--;

        if (leaseCompanyHouse.PlayerDuties <= 0)
        {
            leaseCompanyHouse.PlayerDuties = 0;

            if (leaseCompanyHouse.HasCashier)
            {
                _pedSyncModule.CreateCashier(leaseCompanyHouse);
            }
        }

        await _houseService.Update(leaseCompanyHouse);
        await _houseModule.UpdateOnClient(leaseCompanyHouse);
    }
}