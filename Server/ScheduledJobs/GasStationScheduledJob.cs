using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Server.Core.ScheduledJobs;
using Server.Database.Enums;
using Server.Modules.WorldMarket;

namespace Server.ScheduledJobs;

public class GasStationScheduledJob : ScheduledJob
{
    private readonly ILogger<GasStationScheduledJob> _logger;
    private readonly Random _random = new();
    private readonly WorldMarketModule _worldMarketModule;


    public GasStationScheduledJob(ILogger<GasStationScheduledJob> logger, WorldMarketModule worldMarketModule) : base(
        TimeSpan.FromHours(2))
    {
        _logger = logger;

        _worldMarketModule = worldMarketModule;
    }

    public override async Task Action()
    {
        _worldMarketModule.FuelPrice = new Dictionary<FuelType, int>
        {
            { FuelType.DIESEL, _random.Next(5, 10) },
            { FuelType.PETROL, _random.Next(5, 10) },
            { FuelType.KEROSENE, _random.Next(5, 10) },
            { FuelType.ELECTRICITY, _random.Next(5, 10) }
        };

        await Task.CompletedTask;
    }
}