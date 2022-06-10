using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Group;

namespace Server.DataAccessLayer.Services;

public class GroupMemberService : BaseService<GroupMemberModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public GroupMemberService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public override async Task<GroupMemberModel?> Find(Expression<Func<GroupMemberModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.GroupMembers.Include(m => m.GroupModel).ThenInclude(g => g.Ranks)
            .Include(m => m.CharacterModel).FirstOrDefaultAsync(expression);
    }
}