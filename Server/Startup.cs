using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using AltV.Net.EntitySync;
using AltV.Net.EntitySync.ServerEvent;
using AltV.Net.EntitySync.SpatialPartitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Refit;
using Server.Core.Abstractions;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities.Factories;
using Server.DataAccessLayer.Context;
using Server.Extensions;
using Server.Modules.Discord;
using Server.Core.ScheduledJobs;

namespace Server;

public class Startup
    : AsyncResource
{
    public IConfiguration Configuration { get; }
    private readonly ServiceProvider _serviceProvider;

    public Startup()
        : base(new ActionTickSchedulerFactory())
    {
        // Read and build configuration
        var basePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

        Configuration = new ConfigurationBuilder()
                        .SetBasePath(basePath)
                        .AddJsonFile("appsettings.json", false, true)
                        .Build();

        // Initialize dependency injection
        var services = new ServiceCollection();

        var loggerFactory = LoggerFactory.Create(builder =>
        {
            builder
                .AddConfiguration(Configuration.GetSection("Logging"));
        });

        var logger = loggerFactory.CreateLogger(typeof(ServiceCollectionExtensions));

        services
            .AddRefitClient<IDiscordApi>()
            .ConfigureHttpClient(c => c.BaseAddress = new Uri("https://discordapp.com/api"));

        services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new JsonStringEnumConverter());
        });

        // Register configuration options
        services.AddOptions();
        services.Configure<AccountOptions>(Configuration.GetSection(nameof(AccountOptions)));
        services.Configure<WeatherOptions>(Configuration.GetSection(nameof(WeatherOptions)));
        services.Configure<VehicleOptions>(Configuration.GetSection(nameof(VehicleOptions)));
        services.Configure<CompanyOptions>(Configuration.GetSection(nameof(CompanyOptions)));
        services.Configure<DeliveryOptions>(Configuration.GetSection(nameof(DeliveryOptions)));
        services.Configure<DevelopmentOptions>(Configuration.GetSection(nameof(DevelopmentOptions)));
        services.Configure<DeathOptions>(Configuration.GetSection(nameof(DeathOptions)));
        services.Configure<WorldLocationOptions>(Configuration.GetSection(nameof(WorldLocationOptions)));
        services.Configure<GameOptions>(Configuration.GetSection(nameof(GameOptions)));
        services.Configure<MdcOptions>(Configuration.GetSection(nameof(MdcOptions)));
        services.Configure<CharacterCreatorOptions>(Configuration.GetSection(nameof(CharacterCreatorOptions)));
        services.Configure<DiscordOptions>(Configuration.GetSection(nameof(DiscordOptions)));

        // Configure and register loggers
        services.AddLogging(config => config
                                      .AddConfiguration(Configuration.GetSection("Logging"))
                                      .AddDebug()
                                      .AddConsole());

        // Register factory for database context
        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
        var useLocal = Configuration.GetSection(nameof(DevelopmentOptions)).GetValue<bool>("LocalDb");
        if (useLocal)
        {
            services.AddDbContextFactory<DatabaseContext>(options => options
                                                                     .UseNpgsql(
                                                                         Configuration.GetConnectionString(
                                                                             "LocalDatabase"),
                                                                         o => o.UseQuerySplittingBehavior(
                                                                             QuerySplittingBehavior.SplitQuery))
                                                                     .EnableSensitiveDataLogging()
                                                                     .ConfigureWarnings(
                                                                         w => w.Throw(
                                                                             RelationalEventId
                                                                                 .MultipleCollectionIncludeWarning)));
        }
        else
        {
            services.AddDbContextFactory<DatabaseContext>(options => options
                                                              .UseNpgsql(
                                                                  Configuration.GetConnectionString("LiveDatabase"),
                                                                  o => o.UseQuerySplittingBehavior(
                                                                      QuerySplittingBehavior.SplitQuery)));
        }

        //Register all dependency injectable services.
        services.AddAllTypes<ITransientScript>();
        services.AddAllTypes<IScopedScript>(ServiceLifetime.Scoped);
        services.AddAllTypes<ISingletonScript>(ServiceLifetime.Singleton);

        // Register all server jobs and scheduled jobs.
        services.AddAllTypes<IJob>();
        services.AddAllTypes<ScheduledJob>();

        // Build DI services.
        _serviceProvider = services.BuildServiceProvider();

        // Everything done
        logger.LogDebug("Dependency Injection initialized successfully.");
    }

    public override void OnStart()
    {
        AltEntitySync.Init(8,
                           syncRate => 200,
                           threadId => false,
                           (threadCount, repository) => new ServerEventNetworkLayer(threadCount, repository),
                           (entity, threadCount) => entity.Type % threadCount,
                           (entityId, entityType, threadCount) => entityType % threadCount,
                           threadId =>
                           {
                               return threadId switch
                               {
                                   // Objects
                                   0 => new LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 64),
                                   // Marker
                                   1 => new LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 64),
                                   // Ped
                                   2 => new LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 64),
                                   // Blips
                                   3 => new LimitedGrid3(50_000, 50_000, 75, 10_000, 10_000, 64),
                                   _ => new LimitedGrid3(50_000, 50_000, 175, 10_000, 10_000, 115)
                               };
                           },
                           new IdProvider());

        // Instantiate startup scripts
        _serviceProvider.InstantiateStartupScripts();

        // Instantiate ServerJobs
        var serverJobs = _serviceProvider.GetServices<IJob>();

        // Execute startup method of all server jobs
        var taskList = new List<Task>();
        Parallel.ForEach(serverJobs, job => taskList.Add(job.OnStartup()));

        // Wait until all jobs finished
        Task.WaitAll(taskList.ToArray());

        // Instantiate scheduled jobs and enable them
        var scheduledJobsManager = _serviceProvider.GetService<ScheduledJobManager>();
        scheduledJobsManager?.EnableWorker();
    }

    public override void OnTick()
    {
        base.OnTick();
    }

    public override void OnStop()
    {
        // Cancel all scheduled jobs
        var scheduledJobsManager = _serviceProvider.GetService<ScheduledJobManager>();
        scheduledJobsManager?.Cancellation.Cancel();

        // Execute shutdown method of all server jobs
        var serverJobs = _serviceProvider.GetServices<IJob>();

        var taskList = new List<Task>();
        Parallel.ForEach(serverJobs, job => taskList.Add(job.OnShutdown()));

        // Wait until all jobs finished
        Task.WaitAll(taskList.ToArray());
    }

    #region Entities

    public override IEntityFactory<IPlayer> GetPlayerFactory()
    {
        return new ServerPlayerFactory();
    }

    public override IEntityFactory<IVehicle> GetVehicleFactory()
    {
        return new ServerVehicleFactory();
    }

    #endregion
}