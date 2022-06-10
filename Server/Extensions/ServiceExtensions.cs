using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Server.Core.Abstractions.ScriptStrategy;

namespace Server.Extensions;

public static class ServiceCollectionExtensions
{
    private static readonly List<Type> ServicesToInstantiate = new();

    public static void AddAllTypes<T>(this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient) where T : class
    {
        #region T is interface

        var typesOfInterface = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.DefinedTypes)
            .Where(x => x.IsClass && !x.IsAbstract && x.GetInterfaces().Contains(typeof(T)));

        foreach (var type in typesOfInterface)
        {
            if (services.Any(e => e.ServiceType == type))
            {
                continue;
            }

            if (type.ImplementedInterfaces.Contains(typeof(ISingletonScript)))
            {
                ServicesToInstantiate.Add(type);
                lifetime = ServiceLifetime.Singleton;
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

        var typesOfClasses = AppDomain.CurrentDomain.GetAssemblies().SelectMany(t => t.GetTypes())
            .Where(x => x.IsClass && !x.IsAbstract && x.IsSubclassOf(typeof(T)));

        foreach (var type in typesOfClasses)
        {
            if (services.Any(e => e.ServiceType == type))
            {
                continue;
            }

            if (type.GetTypeInfo().ImplementedInterfaces.Contains(typeof(ISingletonScript)))
            {
                ServicesToInstantiate.Add(type);
                lifetime = ServiceLifetime.Singleton;
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

    public static void InstantiateStartupScripts(this ServiceProvider provider)
    {
        foreach (var type in ServicesToInstantiate)
        {
            _ = provider.GetService(type);
        }
    }
}