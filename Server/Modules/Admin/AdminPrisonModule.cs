using System.Threading.Tasks;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Logging;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.CustomLogs;
using Server.Modules.Character;

namespace Server.Modules.Admin;

public class AdminPrisonModule : ITransientScript
{
    private readonly AccountService _accountService;

    private readonly CharacterSelectionModule _characterSelectionModule;
    private readonly ILogger<AdminPrisonModule> _logger;

    private readonly UserRecordLogService _userRecordLogService;

    public AdminPrisonModule(ILogger<AdminPrisonModule> logger, UserRecordLogService userRecordLogService,
        AccountService accountService, CharacterSelectionModule characterSelectionModule)
    {
        _logger = logger;

        _userRecordLogService = userRecordLogService;
        _accountService = accountService;

        _characterSelectionModule = characterSelectionModule;
    }

    public async Task SetPlayerInPrison(ServerPlayer player, ServerPlayer? staffPlayer, string reason = "")
    {
        if (!player.Exists)
        {
            return;
        }

        await AltAsync.Do(() =>
        {
            lock (player)
            {
                player.Model = 0x6AF51FAF;
                player.SetUniqueDimension();
                player.SetPosition(404.80878f, 6493.042f, 27.5f);

                player.EmitLocked("adminprison:start", player.AccountModel.AdminCheckpoints);
            }
        });

        // adminPlayer is null when the method gets called from auth. module because player has checkpoints left.
        if (staffPlayer != null)
        {
            await _userRecordLogService.Add(new UserRecordLogModel
            {
                AccountModelId = player.AccountModel.SocialClubId,
                StaffAccountModelId = staffPlayer.AccountModel.SocialClubId,
                CharacterModelId = player.CharacterModel.Id,
                UserRecordType = UserRecordType.AUTOMATIC,
                Text = "Spieler wurde für " + player.AccountModel.AdminCheckpoints +
                       " Checkpoints mit dem Grund '" + reason + "' in das Admin Prison gesteckt."
            });
        }
    }

    public async Task ClearPlayerFromPrison(ServerPlayer player, ServerPlayer? staffPlayer)
    {
        if (!player.Exists)
        {
            return;
        }

        player.AccountModel.AdminCheckpoints = 0;
        await _accountService.Update(player.AccountModel);

        player.EmitLocked("adminprison:stop");

        // adminPlayer is null when the method gets called from request checkpoint method because player has checkpoints left.
        if (staffPlayer != null)
        {
            await _userRecordLogService.Add(new UserRecordLogModel
            {
                AccountModelId = player.AccountModel.SocialClubId,
                StaffAccountModelId = staffPlayer.AccountModel.SocialClubId,
                CharacterModelId = player.CharacterModel.Id,
                UserRecordType = UserRecordType.AUTOMATIC,
                Text = "Spieler wurde aus dem Admin Prison befreit."
            });
        }

        await _characterSelectionModule.OpenAsync(player);
    }
}