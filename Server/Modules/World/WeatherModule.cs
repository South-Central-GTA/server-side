using System;
using System.Collections.Generic;
using AltV.Net.Async;
using AltV.Net.Enums;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;

namespace Server.Modules.World;

public class WeatherModule : ISingletonScript
{
    private readonly ILogger<WeatherModule> _logger;

    private readonly Random _random = new();

    private readonly Dictionary<WeatherType, Dictionary<WeatherType, float>> _weatherDefinitions = new()
    {
        {
            WeatherType.ExtraSunny,
            new Dictionary<WeatherType, float>
            {
                { WeatherType.Clouds, 0.4f },
                { WeatherType.Clear, 0.3f },
                { WeatherType.Overcast, 0.1f },
                { WeatherType.Foggy, 0.1f },
                { WeatherType.Smog, 0.1f }
            }
        },
        {
            WeatherType.Clear,
            new Dictionary<WeatherType, float>
            {
                { WeatherType.Clouds, 0.4f },
                { WeatherType.ExtraSunny, 0.4f },
                { WeatherType.Foggy, 0.1f },
                { WeatherType.Overcast, 0.1f }
            }
        },
        {
            WeatherType.Clouds,
            new Dictionary<WeatherType, float>
            {
                { WeatherType.Clear, 0.8f }, { WeatherType.Overcast, 0.2f }, { WeatherType.Foggy, 0.2f }
            }
        },
        {
            WeatherType.Smog,
            new Dictionary<WeatherType, float> { { WeatherType.Clear, 0.8f }, { WeatherType.Clouds, 0.1f } }
        },
        {
            WeatherType.Overcast,
            new Dictionary<WeatherType, float>
            {
                { WeatherType.Rain, 0.5f }, { WeatherType.Thunder, 0.3f }, { WeatherType.Clearing, 0.2f }
            }
        },
        {
            WeatherType.Rain,
            new Dictionary<WeatherType, float>
            {
                { WeatherType.Clearing, 0.6f }, { WeatherType.Thunder, 0.2f }, { WeatherType.Rain, 0.2f }
            }
        },
        {
            WeatherType.Thunder,
            new Dictionary<WeatherType, float>
            {
                { WeatherType.Clearing, 0.8f }, { WeatherType.Rain, 0.1f }, { WeatherType.Thunder, 0.1f }
            }
        },
        {
            WeatherType.Clearing,
            new Dictionary<WeatherType, float>
            {
                { WeatherType.Clear, 0.7f }, { WeatherType.Clouds, 0.2f }, { WeatherType.Smog, 0.1f }
            }
        }
    };

    private readonly WorldData _worldData;

    public int SecondsToChangeWeather;

    public WeatherModule(ILogger<WeatherModule> logger, WorldData worldData)
    {
        _logger = logger;
        _worldData = worldData;
    }

    public void SetWeather(WeatherType weather, int timeFrame)
    {
        _worldData.Weather = weather;
        AltAsync.EmitAllClients("weather:updateweather", timeFrame);
    }

    public void GetRandomWeather()
    {
        var foundCycle = _weatherDefinitions.TryGetValue(_worldData.Weather, out var weathers);
        if (!foundCycle)
        {
            weathers = _weatherDefinitions[WeatherType.ExtraSunny];
        }

        _worldData.Weather = SelectRandomWeather(weathers);

        var transTime = _random.Next(5, 15);
        SecondsToChangeWeather = 60 * transTime;

        AltAsync.EmitAllClients("weather:updateweather", SecondsToChangeWeather);
        AltAsync.Log($"WeatherService: Current weather: {_worldData.Weather}, transition time: {transTime} minutes.");
    }

    private WeatherType SelectRandomWeather(Dictionary<WeatherType, float> weathers)
    {
        var randomNumber = _random.NextDouble();

        foreach (var (key, value) in weathers)
        {
            if (randomNumber < value)
            {
                return key;
            }

            randomNumber -= value;
        }

        AltAsync.Log("WeatherService: Fallback weather got selected, something is wrong.");
        return WeatherType.Clear;
    }
}