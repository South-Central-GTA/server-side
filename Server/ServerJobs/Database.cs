using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Npgsql;
using Server.Core.Abstractions;
using Server.Core.Callbacks;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.DataAccessLayer.Context;
using Server.Database.Enums;
using Server.Database.Models;
using Server.Database.Models.Character;
using Server.Database.Models.Group;

namespace Server.ServerJobs;

public class Database : IServerJob
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;
    private readonly DevelopmentOptions _devOptions;
    private readonly ILogger<Database> _logger;

    public Database(
        ILogger<Database> logger,
        IDbContextFactory<DatabaseContext> dbContextFactory,
        IOptions<DevelopmentOptions> devOptions)
    {
        _logger = logger;
        _dbContextFactory = dbContextFactory;
        _devOptions = devOptions.Value;
    }

    public Task OnSave()
    {
        var playersTask = Task.Run(async () =>
        {
            var charsToUpdate = new List<CharacterModel>();
            var callback = new AsyncFunctionCallback<IPlayer>(async player =>
            {
                var serverPlayer = (ServerPlayer)player;

                if (serverPlayer.IsSpawned)
                {
                    serverPlayer.CharacterModel.Position = serverPlayer.Position;
                    serverPlayer.CharacterModel.Rotation = serverPlayer.Rotation;

                    serverPlayer.CharacterModel.Health = serverPlayer.Health;
                    serverPlayer.CharacterModel.Armor = serverPlayer.Armor;

                    charsToUpdate.Add(serverPlayer.CharacterModel);
                }

                await Task.CompletedTask;
            });

            await Alt.ForEachPlayers(callback);

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
            dbContext.Characters.UpdateRange(charsToUpdate);
            await dbContext.SaveChangesAsync();
        });

        return Task.WhenAll(playersTask);
    }

    public async Task OnShutdown()
    {
        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();

        if (_devOptions.DropDatabaseAtStartup)
        {
            if (!_devOptions.LocalDb)
            {
                await dbContext.Database.ExecuteSqlRawAsync("GRANT CONNECT ON DATABASE scdb TO public;");
            }

            await dbContext.Database.EnsureDeletedAsync();
            _logger.LogWarning("Database dropped.");
        }

        await dbContext.Database.MigrateAsync();

        if (_devOptions.SeedingDefaultDataIntoDatabase)
        {
            var basePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Data");
            
            await dbContext.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync($"{basePath}/Animations.sql"));
            await dbContext.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync($"{basePath}/VehicleCatalog.sql"));
            await dbContext.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync($"{basePath}/ItemCatalog.sql"));
            await dbContext.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync($"{basePath}/Houses.sql"));
            await dbContext.Database.ExecuteSqlRawAsync(await File.ReadAllTextAsync($"{basePath}/Doors.sql"));

            await dbContext.SaveChangesAsync();

            _logger.LogInformation("Seed default data.");
        }

        await Task.CompletedTask;
    }
}