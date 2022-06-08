using AltV.Net.Async;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Modules.Houses;

namespace Server.Handlers.CharacterCreator;

public class CharacterCreatorHouseHandler : ISingletonScript
{
    private readonly CharacterCreatorOptions _characterCreatorOptions;

    private readonly HouseModule _houseModule;

    public CharacterCreatorHouseHandler(
        IOptions<CharacterCreatorOptions> characterCreatorOptions,
        HouseModule houseModule)
    {
        _characterCreatorOptions = characterCreatorOptions.Value;
        _houseModule = houseModule;

        AltAsync.OnClient<ServerPlayer>("charcreatorhouse:open", OnOpenHouseMenu);
        AltAsync.OnClient<ServerPlayer, int>("houseselector:tryselect", OnTrySelectHouse);
        AltAsync.OnClient<ServerPlayer>("houseselector:unselect", OnUnselectHouse);
    }

    private async void OnOpenHouseMenu(ServerPlayer player)
    {
        player.EmitLocked("houseselector:open", _characterCreatorOptions.MaxSouthCentralPointsHouses);
    }

    private async void OnTrySelectHouse(ServerPlayer player, int houseId)
    {
        await _houseModule.SelectHouseInCreation(player, houseId);
        await _houseModule.UpdateHouses();
    }

    private async void OnUnselectHouse(ServerPlayer player)
    {
        _houseModule.UnselectHouseInCreation(player, true);
        await _houseModule.UpdateHouses();
    }
}