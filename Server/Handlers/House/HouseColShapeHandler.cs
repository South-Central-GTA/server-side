using System;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Houses;

namespace Server.Handlers.House;

public class HouseColShapeHandler : ISingletonScript
{
    private readonly HouseModule _houseModule;
    private readonly HouseService _houseService;

    public HouseColShapeHandler(
        HouseService houseService,
        HouseModule houseModule)
    {
        _houseService = houseService;
        _houseModule = houseModule;

        AltAsync.OnColShape += OnColShape;
    }

    private async Task OnColShape(IColShape colShape, IEntity targetEntity, bool state)
    {
        if (targetEntity is ServerPlayer player)
        {
            if (!player.Exists)
            {
                return;
            }

            if (!_houseModule.ColShapes.ContainsValue(colShape))
            {
                return;
            }

            var houseId = _houseModule.ColShapes.FirstOrDefault(x => x.Value == colShape).Key;
            var house = await _houseService.GetByKey(houseId);
            if (house?.InteriorId == null)
            {
                return;
            }

            if (state)
            {
                switch (house.HouseType)
                {
                    case HouseType.HOUSE:
                        player.SendSubtitle("Du kannst mit Taste ~y~F~w~ mit der Tür interagieren.", 3000);
                        break;
                    case HouseType.COMPANY:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            else
            {
                player.ClearSubtitle();
            }
        }
    }
}