using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using Discord;
using Discord.Rest;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models;
using Server.Helper;
using Server.Modules.Chat;

namespace Server.Modules.Discord;

public class DiscordModule : ISingletonScript
{
    private readonly AccountService _accountService;
    private readonly CommandModule _commandModule;
    private readonly DiscordOptions _discordOptions;
    private readonly ILogger<DiscordModule> _logger;
    private readonly IDiscordApi _discordApi;
    private readonly Serializer _serializer;

    private readonly ulong[] _whitelistRoleIds;

    private DiscordSocketClient _client;

    private readonly ulong[] _notLoggedChannels = { 653892719562850327, 838485945640157215, 797060947503087636, 837621889866137620, 767725204252131358, 834439331262234685, 835938380591529984, 837607641302695936, 833319354023804959, 831430838532702208, 849300632715919381, 684018624167542814, 568457139622903820, 826443913689563186, 915224578698125332, 831432353649000488, 932394258034487416 };

    private SocketGuild _serverGuild;

    public DiscordModule(
        ILogger<DiscordModule> logger,
        IOptions<DiscordOptions> discordOptions,
        CommandModule commandModule,
        AccountService accountService, 
        IDiscordApi discordApi, 
        Serializer serializer)
    {
        _logger = logger;
        _discordOptions = discordOptions.Value;

        _commandModule = commandModule;

        _accountService = accountService;
        _discordApi = discordApi;
        _serializer = serializer;

        _whitelistRoleIds = new[] { _discordOptions.StaffUserRoleId, _discordOptions.TesterUserRoleId };

        Connect().Wait();
    }

    public bool CanJoin(ulong playerDiscordId)
    {
        var userClient = _serverGuild.GetUser(playerDiscordId);

        if (userClient == null)
        {
            return false;
        }

        return userClient.Roles.Any(r => _whitelistRoleIds.Contains(r.Id));
    }

    public void SendDiscordMessage(AccountModel accountModel, string title, string description, Color color)
    {
        var userClient = _serverGuild.GetUser(accountModel.DiscordId);

        var embed = new EmbedBuilder();
        embed.WithColor(color)
             .WithTitle(title)
             .WithDescription(description)
             .WithImageUrl("https://sc-rp.de/images/scrp/logo.png");

        userClient.SendMessageAsync(null, false, embed.Build());
    }

    public string? GetAvatar(ulong playerDiscordId)
    {
        if (!_serverGuild.IsConnected)
        {
            return "";
        }

        var userClient = _serverGuild.GetUser(playerDiscordId);
        return userClient?.GetAvatarUrl();
    }

    public string? GetName(ulong playerDiscordId)
    {
        if (!_serverGuild.IsConnected)
        {
            return "";
        }

        var userClient = _serverGuild.GetUser(playerDiscordId);
        return userClient?.Username;
    }

    public async Task UpdatePermissions(ServerPlayer serverPlayer)
    {
        var userClient = _serverGuild.GetUser(serverPlayer.DiscordId);

        var player = Alt.GetAllPlayers().FindPlayerByDiscordId(userClient.Id);
        if (player is not { Exists: true, IsLoggedIn: true })
        {
            return;
        }

        if (userClient.Username != player.AccountModel.CurrentName)
        {
            player.AccountModel.NameHistory.Add(player.AccountModel.CurrentName);
            if (player.AccountModel.NameHistory.Count > 5)
            {
                player.AccountModel.NameHistory.RemoveAt(0);
            }

            player.AccountModel.CurrentName = userClient.Username;
        }

        player.AccountModel.Permission = 0;

        foreach (var role in userClient.Roles)
        {
            if (_discordOptions.OwnerUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.OWNER;
            }
            else if (_discordOptions.LeadAdminUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.LEAD_AMIN;
            }
            else if (_discordOptions.FounderFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.FOUNDER;
            }
            else if (_discordOptions.AdminUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.ADMIN;
            }
            else if (_discordOptions.ModUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.MOD;
            }
            else if (_discordOptions.DevUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.DEV;
            }
            else if (_discordOptions.StaffUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.STAFF;
            }
            else if (_discordOptions.TesterUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.TESTER;
            }
            else if (_discordOptions.FactionManagementFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.FACTION_MANAGEMENT;
            }
            else if (_discordOptions.HeadOfFactionManagementFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.HEAD_FACTION_MANAGEMENT;
            }
            else if (_discordOptions.TeamManagementFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.TEAM_MANAGEMENT;
            }
            else if (_discordOptions.HeadOfTeamManagementFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.HEAD_TEAM_MANAGEMENT;
            }
            else if (_discordOptions.EconomyManagementFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.ECONOMY_MANAGEMENT;
            }
            else if (_discordOptions.HeadOfEconomyManagementFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.HEAD_ECONOMY_MANAGEMENT;
            }
            else if (_discordOptions.LoreAndEventManagementFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.LORE_AND_EVENT_MANAGEMENT;
            }
            else if (_discordOptions.HeadOfLoreAndEventManagementFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.HEAD_LORE_AND_EVENT_MANAGEMENT;
            }
            else if (_discordOptions.FounderFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.LORE_AND_EVENT_MANAGEMENT;
            }
            else if (_discordOptions.ManageAnimationsFlagUserRoleId == role.Id)
            {
                player.AccountModel.Permission |= Permission.MANAGE_ANIMATIONS;
            }
        }

        await _accountService.Update(player.AccountModel);

        player.EmitLocked("chat:setcommands", _commandModule.GetAllCommand(player));
        player.EmitLocked("account:setpermissions", (int)player.AccountModel.Permission);
    }

    private async Task Connect()
    {
        var config = new DiscordSocketConfig { MessageCacheSize = 500 };
        _client = new DiscordSocketClient(config);

        //_client.Log += OnLog;
        _client.GuildAvailable += OnGuildAvailable;
        _client.ReactionAdded += OnReactionAdded;
        _client.ReactionRemoved += OnReactionRemoved;
        _client.MessageReceived += OnMessageReceived;
        _client.MessageDeleted += OnMessageDeleted;
        _client.MessageUpdated += OnMessageUpdated;

        await _client.LoginAsync(TokenType.Bot, _discordOptions.Token);
        await _client.StartAsync();
    }

    private async Task OnGuildAvailable(SocketGuild server)
    {
        _serverGuild = server;
    }

    private async Task OnReactionAdded(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel socketMessageChannel, SocketReaction reaction)
    {
        if (message.Id == _discordOptions.WecomeMessageId && reaction.Emote.Name == "✅")
        {
            var role = _serverGuild.GetRole(_discordOptions.VerifiedUserRoleId);
            var userId = reaction.UserId;
            var user = _serverGuild.GetUser(userId);

            user?.AddRoleAsync(role);
        }
    }

    private async Task OnReactionRemoved(Cacheable<IUserMessage, ulong> message, ISocketMessageChannel socketMessageChannel,
                                         SocketReaction reaction)
    {
        if (message.Id == _discordOptions.WecomeMessageId && reaction.Emote.Name == "✅")
        {
            var role = _serverGuild.GetRole(_discordOptions.VerifiedUserRoleId);
            var userId = reaction.UserId;
            var user = _serverGuild.GetUser(userId);

            user?.RemoveRoleAsync(role);
        }
    }

    private async Task OnMessageReceived(SocketMessage message)
    {
        if (!message.Content.StartsWith("!")
            || message.Author.IsBot
            || !_discordOptions.IsLive)
        {
            return;
        }

        var lengthOfCommand = -1;

        // Get the length, if it has spaces check for the space and get the index if not take the content length.
        lengthOfCommand = message.Content.Contains(" ")
            ? message.Content.IndexOf(" ", StringComparison.Ordinal)
            : message.Content.Length;

        var command = message.Content.Substring(1, lengthOfCommand - 1);

        if (message.Channel.Id == 831614918268682270) // #bot-talk
        {
            switch (command)
            {
                case "players":
                {
                    var players = Alt.GetAllPlayers().Select(player => player.Name).ToArray();
                    var embedBuilder = new EmbedBuilder();

                    if (players.Length != 0)
                    {
                        embedBuilder.WithColor(Color.LightGrey)
                                    .WithTitle("Spielerliste")
                                    .WithDescription($"{string.Join(", ", players)}");
                    }
                    else
                    {
                        embedBuilder.WithColor(Color.LightGrey)
                                    .WithTitle("Spielerliste")
                                    .WithDescription("Leider ist der Server aktuell leer.");
                    }

                    await message.Channel.SendMessageAsync(null, false, embedBuilder.Build());
                    break;
                }
            }
        }

        if (message.Author.Id == 223074682344177664) // <- Pride
        {
            if (command.Equals("update"))
            {
                var welcomeMessage =
                    await message.Channel.GetMessageAsync(_discordOptions.WecomeMessageId) as RestUserMessage;

                var embedBuilder = new EmbedBuilder();
                if (welcomeMessage != null)
                {
                    await welcomeMessage.ModifyAsync(x =>
                    {
                        x.Embed = embedBuilder.WithColor(Color.LightGrey)
                                              .WithTitle("Herzlich Willkommen auf South Central")
                                              .WithDescription("Wir sind ein deutscher chatbasierter Rollenspiel Server in Grand Theft Auto:V, und nutzen die Multiplayer Modifikation alt:V.\n\n" +
                                                               "Solltest du mit uns quatschen wollen, besuche uns auf unserem Teamspeak³ Server: ts.sc-rp.de - ansonsten findest du hier im Discord alle nötigen Informationen.\n\n" +
                                                               "1. Wir spielen zwar in einem recht rauen Stadtbild jedoch verhalten wir uns im Discord nicht so, behandle andere so, wie du von ihnen behandelt werden willst.\n" +
                                                               "2. Jeglicher Inhalt von pornografischen, beleidigenden, rassistischen, extremistischen, anfeindenden, anstößigen u.o. diskriminierenden Medien, Themen u.o. Beiträgen ist auf allen Plattformen verboten.\n" +
                                                               "3. Das erwähnen oder werben für andere Projekte jeglicher Art ist ohne ausdrücklicher Erlaubnis eines Team Mitgliedes verboten. Ausnahme hier ist der Discord Status.\n" +
                                                               "4. Grand Theft Auto: V ist in Deutschland ab dem achtzehnten (18) Lebensjahr erhältlich wir empfehlen dieses Alter für unsere Community.\n" +
                                                               "   Solltet ihr nicht laut deutschem Gesetz Volljährig sein und dennoch hier spielen, übernehmen wir keine Verantwortung oder gar Haftung.\n" +
                                                               "5. Achte auf deinen Umgangston, egal auf welcher Plattform von uns.\n" +
                                                               "6. Auf dem Discord ist nur Deutsch als Sprache erlaubt.\n" +
                                                               "7. Auf all unseren Plattformen ist es untersagt mit einem VPN / Proxy sich zu verbinden.\n" +
                                                               "8. Auf all unseren Plattformen beziehen wir uns auf §§ 858, 903, 1004 BGB in Anwendung des Virtuellen Hausrechtes.\n" +
                                                               "9. Shitposts ohne jeglichen sinnvollen Inhalt welche zum trollen, triggern oder denunzieren von anderen Mitgliedern dienen, sind verboten.\n\n(Zuletzt geupdated 16.05.2021 22:26)")
                                              .WithFooter("*Reagiere auf diesen Post mit dem ✅ um unsere allgemeinen Regeln zu akzeptieren und aktiviert zu werden.*")
                                              .WithImageUrl("https://images.sc-rp.de/logo.png")
                                              .Build();
                    });

                    // await welcomeMessage.AddReactionAsync(new Emoji("✅"));
                }
            }
        }
    }

    private async Task OnMessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel socketMessageChannel)
    {
        if (!msg.HasValue)
        {
            return;
        }

        if (_notLoggedChannels.Contains(msg.Value.Channel.Id))
        {
            return;
        }

        if (_client.GetChannel(653892719562850327) is not IMessageChannel logChannel)
        {
            return;
        }

        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithColor(Color.LightGrey)
                    .WithTitle("Nachrichten Log")
                    .WithDescription(msg.Value.Author.Username + " hat eine Nachricht im Channel " + "'" +
                                     msg.Value.Channel.Name + "' gelöscht.")
                    .WithFooter(msg.Value.Content);

        await logChannel.SendMessageAsync(null, false, embedBuilder.Build());
    }

    private async Task OnMessageUpdated(Cacheable<IMessage, ulong> before, SocketMessage after,
                                        ISocketMessageChannel channel)
    {
        if (!before.HasValue || _notLoggedChannels.Contains(channel.Id))
        {
            return;
        }

        if (_client.GetChannel(653892719562850327) is not IMessageChannel logChannel)
        {
            return;
        }

        if (before.Value.Content == after.Content)
        {
            return;
        }

        var embedBuilder = new EmbedBuilder();
        embedBuilder.WithColor(Color.LightGrey)
                    .WithTitle("Nachrichten Log")
                    .WithDescription(before.Value.Author.Username + " hat eine Nachricht im Channel " + "'" +
                                     before.Value.Channel.Name + "' bearbeitet.")
                    .WithFooter("Original: " + before.Value.Content + "\n\nBearbeitet: " + after.Content);

        await logChannel.SendMessageAsync(null, false, embedBuilder.Build());
    }

    public async Task<DiscordUserDto> AuthenticatePlayer(ServerPlayer player, string token)
    {
        var userResponseJson = await _discordApi.GetUser(token);
        if (string.IsNullOrEmpty(userResponseJson.Content))
        {
            throw new NullReferenceException();
        }
        
        return _serializer.Deserialize<DiscordUserDto>(userResponseJson.Content);
    }
}