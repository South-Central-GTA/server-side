using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Inventory.Phone;

namespace Server.DataAccessLayer.Services;

public class PhoneChatService : BaseService<PhoneChatModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public PhoneChatService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<PhoneChatModel?> GetByKey(int chatId)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.PhoneChats.Include(c => c.Messages).FirstOrDefaultAsync(p => p.Id == chatId);
    }

    public override async Task<PhoneChatModel?> Find(Expression<Func<PhoneChatModel, bool>> expression)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.PhoneChats.Include(c => c.Messages).FirstOrDefaultAsync(expression);
    }
}