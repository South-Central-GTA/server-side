using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.Mail;

namespace Server.DataAccessLayer.Services;

public class MailAccountCharacterAccessService : BaseService<MailAccountCharacterAccessModel>, ITransientScript
{
    public MailAccountCharacterAccessService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(
        dbContextFactory)
    {
    }
}