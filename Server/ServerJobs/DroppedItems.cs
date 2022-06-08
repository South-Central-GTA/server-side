using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions;
using Server.Core.Configuration;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Helper;
using Server.Modules.EntitySync;

namespace Server.ServerJobs;

public class DroppedItems : IJob
{
    private readonly DevelopmentOptions _developmentOptions;
    private readonly GameOptions _gameOptions;

    private readonly ItemService _itemService;
    private readonly ILogger<DroppedItems> _logger;

    private readonly ObjectSyncModule _objectSyncModule;
    private readonly Serializer _serializer;

    public DroppedItems(
        ILogger<DroppedItems> logger,
        IOptions<DevelopmentOptions> developmentOptions,
        IOptions<GameOptions> gameOptions,
        Serializer serializer,
        ItemService itemService,
        ObjectSyncModule objectSyncModule)
    {
        _logger = logger;

        _developmentOptions = developmentOptions.Value;
        _gameOptions = gameOptions.Value;
        _serializer = serializer;

        _itemService = itemService;

        _objectSyncModule = objectSyncModule;
    }

    public async Task OnSave()
    {
        await Task.CompletedTask;
    }

    public async Task OnShutdown()
    {
        _objectSyncModule.DeleteAll();
        await Task.CompletedTask;
    }

    public async Task OnStartup()
    {
        if (_developmentOptions.DropDatabaseAtStartup)
        {
            return;
        }

        var items = await _itemService.GetAll();

        var droppedItems = items.Where(i => i.ItemState == ItemState.DROPPED).ToList();

        var itemsToDelete = droppedItems.Where(i => (i.LastUsage - DateTime.Now).TotalDays >=
                                                    _gameOptions.DeleteDroppedItemsAfterDays);

        droppedItems.RemoveAll(i => (i.LastUsage - DateTime.Now).TotalDays >= _gameOptions.DeleteDroppedItemsAfterDays);

        await _itemService.RemoveRange(itemsToDelete);

        foreach (var droppedItem in droppedItems)
        {
            _objectSyncModule.Create(droppedItem.CatalogItemModel.Model,
                                     droppedItem.CatalogItemModel.Name,
                                     droppedItem.Position,
                                     droppedItem.CatalogItemModel.Rotation,
                                     droppedItem.Dimension,
                                     200,
                                     true,
                                     false,
                                     droppedItem.Id,
                                     droppedItem.DroppedByCharacter,
                                     _serializer.Serialize(droppedItem.CreatedAt));
        }

        await Task.CompletedTask;
    }
}