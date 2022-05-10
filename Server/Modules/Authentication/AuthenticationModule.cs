using System;
using System.Threading.Tasks;
using System.Timers;
using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.DataAccessLayer.Services;
using Server.Database.Models;
using Server.Modules.Admin;
using Server.Modules.Character;
using Server.Modules.Discord;
using Server.Modules.Houses;

namespace Server.Modules.Authentication;

public class AuthenticationModule : ISingletonScript
{
    private readonly AccountOptions _accOptions;

    private readonly AccountService _accountService;
    private readonly AdminPrisonModule _adminPrisonModule;
    private readonly CharacterSelectionModule _characterSelectionModule;

    private readonly DiscordModule _discordModule;
    private readonly HouseModule _houseModule;
    private readonly ILogger<AuthenticationModule> _logger;

    public AuthenticationModule(
        ILogger<AuthenticationModule> logger,
        IOptions<AccountOptions> accOptions,
        AccountService accountService,
        DiscordModule discordModule,
        CharacterSelectionModule characterSelectionModule,
        HouseModule houseModule,
        AdminPrisonModule adminPrisonModule)
    {
        _logger = logger;
        _accOptions = accOptions.Value;

        _accountService = accountService;

        _discordModule = discordModule;
        _characterSelectionModule = characterSelectionModule;
        _houseModule = houseModule;
        _adminPrisonModule = adminPrisonModule;
    }

    public async Task SignIn(ServerPlayer player, string rawPassword)
    {
        if (!player.Exists)
        {
            return;
        }

        var account = await _accountService.Find(a => a.SocialClubId == player.SocialClubId);
        if (account == null)
        {
            player.EmitGui("signin:showerror", "Es wurde kein Account unter diesem GTA gefunden, bitte starte dein Spiel neu.");
            return;
        }

        var verified = BCrypt.Net.BCrypt.Verify(rawPassword, account.PasswordHash);

        if (!verified)
        {
            player.EmitGui("signin:showerror", "Das Passwort ist falsch.");
            return;
        }

        // // Check for faking authentication
        // if (account.SocialClubId != player.SocialClubId
        //     || account.HardwareIdHash != player.HardwareIdHash
        //     || account.HardwareIdExHash != player.HardwareIdExHash)
        // {
        //     player.Kick("Die abgespeicherten Sicherheitsdaten wie Social Club ID und weitere Merkmale sind nicht mehr gleich, der Account gehört nicht zu deinem GTA.");
        //     return;
        // }

        await ContinueLoginProcess(player, account);
    }

    public async Task ChangePassword(ServerPlayer player, string rawoldPassword, string rawNewPassword)
    {
        if (!player.Exists)
        {
            return;
        }

        var account = await _accountService.Find(a => a.SocialClubId == player.SocialClubId);
        if (account == null)
        {
            player.Kick("Es wurde kein Account mehr gefunden, du wurdest zur Sicherheit gekickt.");
            return;
        }

        var verified = BCrypt.Net.BCrypt.Verify(rawoldPassword, account.PasswordHash);

        if (!verified)
        {
            player.EmitGui("passwordchangedialog:wrongoldpassword");
            return;
        }

        if (account.SocialClubId != player.SocialClubId
            || account.HardwareIdHash != player.HardwareIdHash
            || account.HardwareIdExHash != player.HardwareIdExHash)
        {
            player.Kick("Die abgespeicherten Sicherheitsdaten wie Social Club ID und weitere Merkmale sind nicht mehr gleich, der Account gehört nicht zu deinem GTA.");
            return;
        }

        player.AccountModel.PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawNewPassword);
        await _accountService.Update(player.AccountModel);

        player.SendNotification("Dein Passwort wurde erfolgreich geändert.", NotificationType.SUCCESS);
        player.EmitGui("passwordchangedialog:changesuccessfully");
    }

    public async Task SignUp(ServerPlayer player, string rawPassword)
    {
        var account = await _accountService.Add(new AccountModel
        {
            MaxCharacters = _accOptions.MaxCharacters,
            SouthCentralPoints = _accOptions.StartSouthCentralPoints,
            SocialClubId = player.SocialClubId,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(rawPassword),
            CurrentName = player.Name,
            DiscordId = player.DiscordId,
            HardwareIdHash = player.HardwareIdHash,
            HardwareIdExHash = player.HardwareIdExHash,
            LastIp = player.Ip, 
            LastSelectedCharacterId = -1,
            OnlineSince = DateTime.Now,
            LastLogin = DateTime.Now,
            BannedUntil = DateTime.MinValue,
            MaxAnimations = 8,
            MaxRoleplayInfos = 6
        });

        await ContinueLoginProcess(player, account);
    }

    public bool VerifyLoginTries(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return false;
        }

        player.LoginTrys++;
        if (player.LoginTrys > _accOptions.LoginTrysMax)
        {
            player.Kick("Der Login-Server hat mehrfache erfolglos Login Versuche von Dir registriert und hat dich gekickt.");

            var loginCheckTimer = new Timer(_accOptions.LoginTrysTimerMinutes * 60000);
            loginCheckTimer.Elapsed += (sender, e) => ResetLoginTries(sender, e, player);
            return false;
        }

        _logger.LogInformation($"Player '{player.DiscordId}' has {player.LoginTrys}/{_accOptions.LoginTrysMax} login tries.");
        return true;
    }

    public async Task ContinueLoginProcess(ServerPlayer player, AccountModel accountModel)
    {
        accountModel.LastLogin = DateTime.Now;
        accountModel.LastIp = player.Ip;
        accountModel.HardwareIdHash = player.HardwareIdHash;
        accountModel.HardwareIdExHash = player.HardwareIdExHash;
        
        accountModel.AvatarUrl = _discordModule.GetAvatar(player.DiscordId);

        if (player.Name != accountModel.CurrentName)
        {
            accountModel.NameHistory.Add(accountModel.CurrentName);
            if (accountModel.NameHistory.Count > 5)
            {
                accountModel.NameHistory.RemoveAt(0);
            }

            accountModel.CurrentName = player.Name;
        }

        player.AccountModel = accountModel;
        await _accountService.Update(accountModel);

        var isBanned = await CheckBanStatus(player);
        if (isBanned)
        {
            return;
        }

        var canJoin = await CheckMaintenanceStatus(player);
        if (!canJoin)
        {
            return;
        }

        await _houseModule.Sync(player);

        if (player.AccountModel.AdminCheckpoints > 0)
        {
            await _adminPrisonModule.SetPlayerInPrison(player, null);
            return;
        }

        _logger.LogInformation($"Player '{player.AccountName}' successfully logged in.");

        await _characterSelectionModule.OpenAsync(player);
    }

    private void ResetLoginTries(object sender, ElapsedEventArgs e, ServerPlayer player)
    {
        player.LoginTrys = 0;
        _logger.LogInformation($"Login tries got resetted for '{player.DiscordId}'.");
    }

    private async Task<bool> CheckMaintenanceStatus(ServerPlayer player)
    {
        // TODO: Create dynamic maintenance system

        var canJoin = _discordModule.CanJoin(player.DiscordId);
        if (!canJoin)
        {
            _logger.LogInformation($"Player '{player.AccountName}' got kicked because whitelist is online.");
            await player.KickAsync("Wir sind noch in der Entwicklung, du bist nicht auf der Whitelist.");
        }

        return canJoin;
    }

    private async Task<bool> CheckBanStatus(ServerPlayer player)
    {
        if (player.AccountModel.BannedUntil != DateTime.MinValue || player.AccountModel.BannedPermanent)
        {
            if (player.AccountModel.BannedUntil > DateTime.Now && !player.AccountModel.BannedPermanent)
            {
                await player.KickAsync("Du wurdest temporär von unserer Community ausgeschlossen!\n" +
                                       $"Grund: {player.AccountModel.BannedReason}\n\n" +
                                       $"Ablauf: {player.AccountModel.BannedUntil:HH:mm:ss dd.MM.yyyy}.");

                return true;
            }

            if (player.AccountModel.BannedPermanent)
            {
                await player.KickAsync("Du wurdest von unserer Community ausgeschlossen!\n" +
                                       $"Grund: {player.AccountModel.BannedReason}");

                return true;
            }

            if (player.AccountModel.BannedUntil <= DateTime.Now && !player.AccountModel.BannedPermanent)
            {
                player.AccountModel.BannedFrom = 0;
                player.AccountModel.BannedReason = null;
                player.AccountModel.BannedUntil = DateTime.MinValue;
                player.AccountModel.BannedPermanent = false;
            }
        }

        return false;
    }
}