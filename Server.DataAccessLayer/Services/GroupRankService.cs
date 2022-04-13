using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Group;

namespace Server.DataAccessLayer.Services;

public class GroupRankService
    : BaseService<GroupRankModel>, ITransientScript
{
    public GroupRankService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
    }
}