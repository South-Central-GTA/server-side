using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Database.Enums;

namespace Server.Modules.WorldMarket;

public class WorldMarketModule : ISingletonScript
{
    private readonly ILogger<WorldMarketModule> _logger;

    public WorldMarketModule(
        ILogger<WorldMarketModule> logger)
    {
        _logger = logger;

        FuelPrice = new Dictionary<FuelType, int>
        {
            { FuelType.DIESEL, 5 }, { FuelType.PETROL, 5 }, { FuelType.KEROSENE, 5 }, { FuelType.ELECTRICITY, 5 }
        };
    }

    public Dictionary<FuelType, int> FuelPrice { get; set; }
}