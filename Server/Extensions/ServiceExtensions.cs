using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;

namespace Server.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly ILogger Logger;
    private static readonly Dictionary<Type, object> SingletonInstances;

    private static readonly List<Type> ServicesToInstantiate = new();

    static ServiceCollectionExtensions()
    {
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        var config = new ConfigurationBuilder()
                     .SetBasePath(basePath)
                     .AddJsonFile("appsettings.json", false, true)
                     .AddJsonFile("appsettings.local.json", true, true)
                     .Build();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConfiguration(config.GetSection("Logging"))
                .AddDebug()
                .AddConsole();
        });

        Logger = loggerFactory.CreateLogger(typeof(ServiceCollectionExtensions));

        SingletonInstances = new Dictionary<Type, object>();
    }

    public static void AddAllTypes<T>(this IServiceCollection services, ServiceLifetime lifetime = ServiceLifetime.Transient)
        where T : class
    {
        #region T is interface

        var typesOfInterface = AppDomain
                               .CurrentDomain
                               .GetAssemblies()
                               .SelectMany(t => t.DefinedTypes)
                               .Where(x => x.IsClass && !x.IsAbstract && x.GetInterfaces().Contains(typeof(T)));

        foreach (var type in typesOfInterface)
        {
            Logger.LogTrace($"Registering {type.Name} (implements interface {typeof(T).Name}) with lifetime {lifetime}");

            if (services.Any(e => e.ServiceType == type))
            {
                Logger.LogDebug($"Skipping registration of {type.Name} -> already registered!");
                continue;
            }

            if (type.ImplementedInterfaces.Contains(typeof(ISingletonScript)))
            {
                ServicesToInstantiate.Add(type);
                lifetime = ServiceLifetime.Singleton;

                Logger.LogTrace($"Configured {type.Name} for instanciation on startup");
            }

            // add as resolvable by implementation type
            services.Add(new ServiceDescriptor(type, type, lifetime));

            if (typeof(T) != type)
            {
                // add as resolvable by service type (forwarding)
                services.Add(new ServiceDescriptor(typeof(T), x => x.GetRequiredService(type), lifetime));
            }
        }

        #endregion

        #region T is class

        var typesOfClasses = AppDomain
                             .CurrentDomain
                             .GetAssemblies()
                             .SelectMany(t => t.GetTypes())
                             .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(T)));

        foreach (var type in typesOfClasses)
        {
            Logger.LogTrace($"Registering {type.Name} (inherits class {typeof(T).Name}) with lifetime {lifetime}");

            if (services.Any(e => e.ServiceType == type))
            {
                Logger.LogDebug($"Skipping registration of {type.Name} -> already registered!");
                continue;
            }

            if (type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISingletonScript)))
            {
                ServicesToInstantiate.Add(type);
                lifetime = ServiceLifetime.Singleton;

                Logger.LogTrace($"Configured {type.Name} for instanciation on startup");
            }

            // add as resolvable by implementation type
            services.Add(new ServiceDescriptor(type, type, lifetime));

            if (typeof(T) != type)
            {
                // add as resolvable by service type (forwarding)
                services.Add(new ServiceDescriptor(typeof(T), x => x.GetRequiredService(type), lifetime));
            }
        }

        #endregion
    }

    private static object GetSingletonInstance(IServiceProvider serviceProvider, Type type)
    {
        if (SingletonInstances.ContainsKey(type))
        {
            return SingletonInstances[type];
        }

        var instance = serviceProvider.GetRequiredService(type);
        SingletonInstances.Add(type, instance);

        return instance;
    }

    public static void InstantiateStartupScripts(this ServiceProvider provider)
    {
        Logger.LogTrace("Dependency Injection: Instanciating registered scripts");

        foreach (var type in ServicesToInstantiate)
        {
            Logger.LogTrace($"GetService {type.Name}");

            _ = provider.GetService(type);

            Logger.LogTrace($"Instantiated {type.Name}");
        }
    }
}