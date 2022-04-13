using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Elements.Args;
using AltV.Net.Elements.Entities;
using AltV.Net.FunctionParser;
using Server.Core.Abstractions;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.CommandSystem;
using Server.Core.Entities;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.CustomLogs;
using Server.Modules;
using Server.Modules.Chat;

namespace Server.ServerJobs;

public class Commands : IServerJob
{
    private static readonly LinkedList<Function> Functions = new();

    private static readonly LinkedList<GCHandle> Handles = new();

    private static readonly string[] EmptyArgs = Array.Empty<string>();

    private readonly IDictionary<string, LinkedList<RestrictedAccessCommandDelegate>> _commandDelegates =
        new Dictionary<string, LinkedList<RestrictedAccessCommandDelegate>>();

    private readonly CommandLogService _commandLogService;
    private readonly CommandModule _commandModule;

    public Commands(
        IEnumerable<IScopedScript> scopedScripts,
        IEnumerable<ISingletonScript> singletonScripts,
        IEnumerable<ITransientScript> transientScripts,
        CommandLogService commandLogSerivce,
        CommandModule commandModules)
    {
        _commandLogService = commandLogSerivce;
        _commandModule = commandModules;

        foreach (var script in scopedScripts)
        {
            RegisterEvents(script);
        }

        foreach (var script in singletonScripts)
        {
            RegisterEvents(script);
        }

        foreach (var script in transientScripts)
        {
            RegisterEvents(script);
        }

        Alt.OnClient<ServerPlayer, string>("command:execute", OnCommandRequest, OnCommandRequestParser);
    }

    private static void OnCommandRequestParser(IPlayer player, MValueConst[] valueArray,
                                               Action<ServerPlayer, string> action)
    {
        if (valueArray.Length != 1)
        {
            return;
        }

        var arg = valueArray[0];
        if (arg.type != MValueConst.Type.String)
        {
            return;
        }

        action((ServerPlayer)player, arg.GetString());
    }

    private async void OnCommandRequest(ServerPlayer player, string command)
    {
        if (string.IsNullOrWhiteSpace(command))
        {
            return;
        }

        var args = command.Split(' ');
        var argsLength = args.Length;

        if (argsLength < 1)
        {
            return; // should never happen
        }

        var cmd = args[0].Trim('/');

        LinkedList<RestrictedAccessCommandDelegate> delegates;
        if (argsLength < 2)
        {
            if (_commandDelegates.TryGetValue(cmd.ToLower(), out delegates) && delegates.Count > 0)
            {
                foreach (var commandDelegate in delegates)
                {
                    if (argsLength - 1 < commandDelegate.ArgsAmount)
                    {
                        foreach (var missingArgsDelegate in CommandMissingArgsDelegates)
                        {
                            missingArgsDelegate(player, cmd);
                            return;
                        }
                    }
                    else
                    {
                        if (player.AccountModel.Permission.HasFlag(commandDelegate.RequiredPermission))
                        {
                            if (commandDelegate.RequiredPermission.HasFlag(Permission.STAFF))
                            {
                                if (player.IsAduty || cmd == "aduty" || cmd == "adminduty")
                                {
                                    commandDelegate.Action(player, EmptyArgs);
                                }
                                else
                                {
                                    foreach (var adutyRequiredDelegate in CommandAdutyRequiredDelegates)
                                    {
                                        adutyRequiredDelegate(player, cmd);
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                commandDelegate.Action(player, EmptyArgs);
                            }
                        }
                        else
                        {
                            foreach (var missingPermissionDelegate in CommandMissingPermissionDelegates)
                            {
                                missingPermissionDelegate(player, cmd);
                                return;
                            }
                        }
                    }

                    await _commandLogService.Add(new CommandLogModel
                    {
                        AccountModelId = player.AccountModel.SocialClubId,
                        CharacterModelId = player.CharacterModel.Id,
                        Name = cmd,
                        Arguments = string.Join(" ", args.Skip(1)),
                        LoggedAt = DateTime.Now,
                        RequiredPermission = commandDelegate.RequiredPermission
                    });
                }
            }
            else
            {
                foreach (var doesNotExistDelegate in CommandDoesNotExistDelegates)
                {
                    doesNotExistDelegate(player, cmd);
                    return;
                }
            }

            return;
        }

        var argsArray = new string[argsLength - 1];
        Array.Copy(args, 1, argsArray, 0, argsLength - 1);
        if (_commandDelegates.TryGetValue(cmd, out delegates) && delegates.Count > 0)
        {
            foreach (var commandDelegate in delegates)
            {
                if (argsLength - 1 < commandDelegate.ArgsAmount)
                {
                    foreach (var missingArgsDelegate in CommandMissingArgsDelegates)
                    {
                        missingArgsDelegate(player, cmd);
                        return;
                    }
                }
                else
                {
                    if (player.AccountModel.Permission.HasFlag(commandDelegate.RequiredPermission))
                    {
                        if (commandDelegate.RequiredPermission.HasFlag(Permission.STAFF))
                        {
                            if (player.IsAduty)
                            {
                                commandDelegate.Action(player, argsArray);
                            }
                            else
                            {
                                foreach (var adutyRequiredDelegate in CommandAdutyRequiredDelegates)
                                {
                                    adutyRequiredDelegate(player, cmd);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            commandDelegate.Action(player, argsArray);
                        }
                    }
                    else
                    {
                        foreach (var missingPermissionDelegate in CommandMissingPermissionDelegates)
                        {
                            missingPermissionDelegate(player, cmd);
                            return;
                        }
                    }
                }

                await _commandLogService.Add(new CommandLogModel { AccountModelId = player.AccountModel.SocialClubId, CharacterModelId = player.CharacterModel.Id, Name = cmd, Arguments = string.Join(" ", args.Skip(1)) });
            }
        }
        else
        {
            foreach (var doesNotExistDelegate in CommandDoesNotExistDelegates)
            {
                doesNotExistDelegate(player, cmd);
                return;
            }
        }
    }

    private void RegisterEvents(object target)
    {
        ModuleScriptMethodIndexer.Index(target,
        new[] { typeof(Command), typeof(CommandEvent) },
        (baseEvent, eventMethod, eventMethodDelegate) =>
        {
            switch (baseEvent)
            {
                case Command command:
                {
                    var commandName = command.Name ?? eventMethod.Name;

                    Handles.AddLast(GCHandle.Alloc(eventMethodDelegate));

                    var function = Function.Create(eventMethodDelegate);
                    if (function == null)
                    {
                        Console.WriteLine($"Unsupported Command method: {eventMethod}");
                        return;
                    }

                    Functions.AddLast(function);

                    _commandModule.AddCommand(command);

                    if (!_commandDelegates.TryGetValue(commandName, out var delegates))
                    {
                        delegates = new LinkedList<RestrictedAccessCommandDelegate>();
                        _commandDelegates[commandName] = delegates;
                    }

                    switch (command.CommandArgs)
                    {
                        case CommandArgs.NOT_GREEDY:
                        {
                            delegates.AddLast(new RestrictedAccessCommandDelegate((player, arguments) =>
                              {
                                  function.Call(player, arguments);
                              },
                              command.RequiredPermission,
                              command.ParameterDescription?.Length ?? 0,
                              command.CommandArgs));
                        }
                            break;
                        case CommandArgs.GREEDY:
                        {
                            delegates.AddLast(new RestrictedAccessCommandDelegate((player, arguments) =>
                              {
                                  function.Call(player, new[] { string.Join(" ", arguments) });
                              },
                              command.RequiredPermission,
                              command.ParameterDescription?.Length ?? 0,
                              command.CommandArgs));
                        }
                            break;
                        case CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT:
                        {
                            delegates.AddLast(new RestrictedAccessCommandDelegate((player, arguments) =>
                              {
                                  function.Call(player, new[] { arguments[0], string.Join(" ", arguments.Skip(1)) });
                              },
                              command.RequiredPermission,
                              command.ParameterDescription?.Length ?? 0,
                              command.CommandArgs));
                        }
                            break;
                        case CommandArgs.GREEDY_BUT_WITH_TWO_FIXED_ARGUMENT:
                        {
                            delegates.AddLast(new RestrictedAccessCommandDelegate((player, arguments) =>
                              {
                                  function.Call(player, new[] { arguments[0], arguments[1], string.Join(" ", arguments.Skip(2)) });
                              },
                              command.RequiredPermission,
                              command.ParameterDescription?.Length ?? 0,
                              command.CommandArgs));
                        }
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }

                    var aliases = command.Aliases;
                    if (aliases != null)
                    {
                        foreach (var alias in aliases)
                        {
                            if (!_commandDelegates.TryGetValue(alias, out delegates))
                            {
                                delegates = new LinkedList<RestrictedAccessCommandDelegate>();
                                _commandDelegates[alias] = delegates;
                            }

                            switch (command.CommandArgs)
                            {
                                case CommandArgs.NOT_GREEDY:
                                {
                                    delegates.AddLast(new RestrictedAccessCommandDelegate((player, arguments) =>
                                      {
                                          function.Call(player, arguments);
                                      },
                                      command.RequiredPermission,
                                      command.ParameterDescription?.Length ?? 0,
                                      command.CommandArgs));
                                }
                                    break;
                                case CommandArgs.GREEDY:
                                {
                                    delegates.AddLast(new RestrictedAccessCommandDelegate((player, arguments) =>
                                      {
                                          function.Call(player, new[] { string.Join(" ", arguments) });
                                      },
                                      command.RequiredPermission,
                                      command.ParameterDescription?.Length ?? 0,
                                      command.CommandArgs));
                                }
                                    break;
                                case CommandArgs.GREEDY_BUT_WITH_ONE_FIXED_ARGUMENT:
                                {
                                    delegates.AddLast(new RestrictedAccessCommandDelegate((player, arguments) =>
                                      {
                                          function.Call(player, new[] { arguments[0], string.Join(" ", arguments.Skip(1)) });
                                      },
                                      command.RequiredPermission,
                                      command.ParameterDescription?.Length ?? 0,
                                      command.CommandArgs));
                                }
                                    break;
                                case CommandArgs.GREEDY_BUT_WITH_TWO_FIXED_ARGUMENT:
                                {
                                    delegates.AddLast(new RestrictedAccessCommandDelegate((player, arguments) =>
                                      {
                                          function.Call(player, new[] { arguments[0], arguments[1], string.Join(" ", arguments.Skip(2)) });
                                      },
                                      command.RequiredPermission,
                                      command.ParameterDescription?.Length ?? 0,
                                      command.CommandArgs));
                                }
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }

                    break;
                }

                case CommandEvent commandEvent:
                {
                    var commandEventType = commandEvent.EventType;
                    ScriptFunction scriptFunction;

                    switch (commandEventType)
                    {
                        case CommandEventType.NOT_FOUND:
                            scriptFunction = ScriptFunction.Create(eventMethodDelegate,
                                                                   new[] { typeof(ServerPlayer), typeof(string) });

                            if (scriptFunction == null)
                            {
                                return;
                            }

                            OnCommandDoesNotExist += (player, commandName) =>
                            {
                                scriptFunction.Set(player);
                                scriptFunction.Set(commandName);
                                scriptFunction.Call();
                            };
                            break;

                        case CommandEventType.MISSING_PERMISSION:
                            scriptFunction = ScriptFunction.Create(eventMethodDelegate,
                                new[] { typeof(ServerPlayer), typeof(string) });

                            if (scriptFunction == null)
                            {
                                return;
                            }

                            OnCommandMissingPermission += (player, commandName) =>
                            {
                                scriptFunction.Set(player);
                                scriptFunction.Set(commandName);
                                scriptFunction.Call();
                            };
                            break;

                        case CommandEventType.ADUTY_REQUIRED:
                            scriptFunction = ScriptFunction.Create(eventMethodDelegate,
                                new[] { typeof(ServerPlayer), typeof(string) });

                            if (scriptFunction == null)
                            {
                                return;
                            }

                            OnCommandAdutyRequired += (player, commandName) =>
                            {
                                scriptFunction.Set(player);
                                scriptFunction.Set(commandName);
                                scriptFunction.Call();
                            };
                            break;

                        case CommandEventType.MISSING_ARGS:
                            scriptFunction = ScriptFunction.Create(eventMethodDelegate,
                                new[] { typeof(ServerPlayer), typeof(string) });

                            if (scriptFunction == null)
                            {
                                return;
                            }

                            OnCommandMissingArgs += (player, commandName) =>
                            {
                                scriptFunction.Set(player);
                                scriptFunction.Set(commandName);
                                scriptFunction.Call();
                            };
                            break;
                    }
                    break;
                }
            }
        });
    }

    private class RestrictedAccessCommandDelegate
    {
        public RestrictedAccessCommandDelegate(CommandDelegate action, Permission requiredPermission, int argsAmount, CommandArgs commandArgs)
        {
            Action = action;
            RequiredPermission = requiredPermission;
            ArgsAmount = argsAmount;
            CommandArgs = commandArgs;
        }

        public CommandDelegate Action { get; }
        public Permission RequiredPermission { get; }
        public int ArgsAmount { get; }
        public CommandArgs CommandArgs { get; }
    }

    private delegate void CommandDelegate(ServerPlayer player, string[] arguments);

    #region IServerJob

    public async Task OnStartup()
    {
        await Task.CompletedTask;
    }

    public async Task OnSave()
    {
        await Task.CompletedTask;
    }

    public async Task OnShutdown()
    {
        Functions.Clear();

        foreach (var handle in Handles)
        {
            handle.Free();
        }

        Handles.Clear();

        await Task.CompletedTask;
    }

    #endregion

    #region CommandDoesNotExists

    private static readonly HashSet<CommandDoesNotExistDelegate> CommandDoesNotExistDelegates =
        new();

    public delegate void CommandDoesNotExistDelegate(ServerPlayer player, string command);

    public static event CommandDoesNotExistDelegate OnCommandDoesNotExist
    {
        add => CommandDoesNotExistDelegates.Add(value);
        remove => CommandDoesNotExistDelegates.Remove(value);
    }

    #endregion

    #region CommandMissingPermission

    private static readonly HashSet<CommandMissingPermissionDelegate> CommandMissingPermissionDelegates =
        new();

    public delegate void CommandMissingPermissionDelegate(ServerPlayer player, string command);

    public static event CommandMissingPermissionDelegate OnCommandMissingPermission
    {
        add => CommandMissingPermissionDelegates.Add(value);
        remove => CommandMissingPermissionDelegates.Remove(value);
    }

    #endregion

    #region CommandAdutyRequired

    private static readonly HashSet<CommandAdutyRequiredDelegate> CommandAdutyRequiredDelegates =
        new();

    public delegate void CommandAdutyRequiredDelegate(ServerPlayer player, string command);

    public static event CommandAdutyRequiredDelegate OnCommandAdutyRequired
    {
        add => CommandAdutyRequiredDelegates.Add(value);
        remove => CommandAdutyRequiredDelegates.Remove(value);
    }

    #endregion

    #region CommandMissingArgs

    private static readonly HashSet<CommandMissingArgsDelegate> CommandMissingArgsDelegates =
        new();

    public delegate void CommandMissingArgsDelegate(ServerPlayer player, string command);

    public static event CommandMissingArgsDelegate OnCommandMissingArgs
    {
        add => CommandMissingArgsDelegates.Add(value);
        remove => CommandMissingArgsDelegates.Remove(value);
    }

    #endregion
}