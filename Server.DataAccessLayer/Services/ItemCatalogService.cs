using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models;

namespace Server.DataAccessLayer.Services;

public class ItemCatalogService
    : BaseService<CatalogItemModel>, ITransientScript
{
    public ItemCatalogService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
    }
}