using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models;

namespace Server.DataAccessLayer.Services;

public class AnimationService
    : BaseService<AnimationModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public AnimationService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }
}