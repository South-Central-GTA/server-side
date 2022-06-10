using Microsoft.EntityFrameworkCore;
using Server.Core.Abstractions.ScriptStrategy;
using Server.DataAccessLayer.Context;
using Server.DataAccessLayer.Services.Base;
using Server.Database.Models.CustomLogs;

namespace Server.DataAccessLayer.Services;

public class ChatLogService : BaseService<ChatLogModel>, ITransientScript
{
    public ChatLogService(IDbContextFactory<DatabaseContext> dbContextFactory) : base(dbContextFactory)
    {
    }
}