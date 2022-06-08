using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;

namespace Server.Helper;

public class Serializer
    : ITransientScript
{
    private readonly ILogger<Serializer> _logger;

    public Serializer(
        ILogger<Serializer> logger)
    {
        _logger = logger;
    }

    public T Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json,
                                             new JsonSerializerOptions
                                             {
                                                 PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                                 PropertyNameCaseInsensitive = true,
                                                 IncludeFields = true,
                                                 NumberHandling = JsonNumberHandling.AllowReadingFromString,
                                                 Converters = { new JsonStringEnumConverter() }
                                             });
    }

    public string Serialize(object obj)
    {
        return JsonSerializer.Serialize(obj,
                                        new JsonSerializerOptions
                                        {
                                            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                                            PropertyNameCaseInsensitive = true,
                                            IncludeFields = true,
                                            NumberHandling = JsonNumberHandling.AllowReadingFromString,
                                            Converters = { new JsonStringEnumConverter() }
                                        });
    }
}