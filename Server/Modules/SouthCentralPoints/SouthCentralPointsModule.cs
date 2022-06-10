using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;

namespace Server.Modules.SouthCentralPoints;

public class SouthCentralPointsModule : ISingletonScript
{
    private readonly GameOptions _gameOptions;

    public SouthCentralPointsModule(IOptions<GameOptions> gameOptions)
    {
        _gameOptions = gameOptions.Value;
    }

    public void ReducePoints(ServerPlayer player, int amount, string reason)
    {
    }

    public int GetPointsPrice(int price)
    {
        var points = (int)(price * _gameOptions.MoneyToPointsExchangeRate);
        return points <= 10 ? 10 : points;
    }
}