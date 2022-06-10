using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Housing;

namespace Server.DataAccessLayer.Services;

public class UserShopDataService : BaseService<UserShopDataModel>, ITransientScript
{
    public UserShopDataService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
    }
}