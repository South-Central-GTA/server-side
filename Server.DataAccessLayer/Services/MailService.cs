using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Mail;

namespace Server.DataAccessLayer.Services;

public class MailService
    : BaseService<MailModel>, ITransientScript
{
    private readonly IDbContextFactory<DatabaseContext> _dbContextFactory;

    public MailService(IDbContextFactory<DatabaseContext> dbContextFactory)
        : base(dbContextFactory)
    {
        _dbContextFactory = dbContextFactory;
    }

    public async Task<MailModel> GetByKey(int id)
    {
        await using var dbContext = await _dbContextFactory.CreateDbContextAsync();
        return await dbContext.Mails
                              .Include(m => m.MailLinks)
                              .ThenInclude(m => m.MailModel)
                              .ThenInclude(ml => ml.MailLinks)
                              .FirstOrDefaultAsync(m => m.Id == id);
    }
}