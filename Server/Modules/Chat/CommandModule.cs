using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;

namespace Server.Modules.Chat;

public class CommandModule : ISingletonScript
{
    private readonly ILogger<CommandModule> _logger;

    private readonly List<Command> _commands = new();

    public CommandModule(
        ILogger<CommandModule> logger)
    {
        _logger = logger;
    }

    public void AddCommand(Command command)
    {
        _commands.Add(command);
        _logger.LogInformation($"Command {command.Name} got added.");
    }

    public List<Command> GetAllCommand(ServerPlayer player)
    {
        return _commands
            .FindAll(c => player.AccountModel.Permission.HasFlag(c.RequiredPermission));
    }
}