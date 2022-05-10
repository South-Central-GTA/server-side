using System.Threading.Tasks;
using AltV.Net.Async;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Bank;

namespace Server.Modules.DefinedJob;

public class DefinedJobModule
    : ITransientScript
{
    private readonly BankAccountService _bankAccountService;
    private readonly BankModule _bankModule;
    private readonly CharacterService _characterService;
    private readonly DefinedJobService _definedJobService;
    private readonly GameOptions _gameOptions;
    private readonly ILogger<DefinedJobModule> _logger;

    public DefinedJobModule(
        IOptions<GameOptions> gameOptions,
        ILogger<DefinedJobModule> logger,
        DefinedJobService definedJobService,
        BankAccountService bankAccountService,
        CharacterService characterService,
        BankModule bankModule)
    {
        _logger = logger;
        _gameOptions = gameOptions.Value;

        _definedJobService = definedJobService;
        _bankAccountService = bankAccountService;
        _characterService = characterService;

        _bankModule = bankModule;
    }

    public async Task<Database.Models.Character.DefinedJobModel?> GetPlayerJob(ServerPlayer player)
    {
        var definedJob = await _definedJobService.Find(j => j.CharacterModelId == player.CharacterModel.Id);
        return definedJob;
    }

    public async Task SelectJob(ServerPlayer player, int jobId, int bankAccountId)
    {
        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        var definedJob = new Database.Models.Character.DefinedJobModel { CharacterModelId = player.CharacterModel.Id, JobId = jobId, BankAccountId = bankAccountId };

        player.CharacterModel.JobModel = definedJob;

        await _definedJobService.Add(definedJob);

        player.SendNotification(
            jobId == 0
                ? "Dein Charakter erhält finanzielle Unterstützung."
                : "Dein Charakter hat erfolgreich ein Berufsfeld ausgewählt.",
            NotificationType.INFO);

        await UpdateUi(player);
    }

    public async Task QuitJob(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var job = await _definedJobService.Find(j => j.CharacterModelId == player.CharacterModel.Id);
        if (job == null)
        {
            return;
        }
        
        await _definedJobService.Remove(job);
        player.CharacterModel.JobModel = null;

        player.SendNotification("Dein Charakter ist wieder arbeitslos und verdient kein Geld mehr.", NotificationType.INFO);
        await UpdateUi(player);
    }

    public async Task ChangeBankAccount(ServerPlayer player, int bankAccountId)
    {
        if (!player.Exists)
        {
            return;
        }
        
        if (player.CharacterModel.JobModel == null)
        {
            return;
        }

        var bankAccount = await _bankAccountService.GetByKey(bankAccountId);
        if (bankAccount == null)
        {
            player.SendNotification("Das Bankkonto existiert nicht mehr.", NotificationType.ERROR);
            return;
        }

        if (!await _bankModule.HasPermission(player, bankAccount, BankingPermission.DEPOSIT))
        {
            player.SendNotification("Dein Charakter hat nicht genügend Berechtigungen mehr um dieses Bankkonto zu nutzen.", NotificationType.ERROR);
            return;
        }

        player.CharacterModel.JobModel.BankAccountId = bankAccountId;
        await _characterService.Update(player);

        player.SendNotification("Das Bankkonto wurde erfolgreich geändert.", NotificationType.SUCCESS);
    }

    public async Task OpenJobMenu(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }
        
        var definedJob = await GetPlayerJob(player);
        DefinedJobData? playerJob = null;
        if (definedJob != null)
        {
            playerJob = _gameOptions.DefinedJobs.Find(j => j.Id == definedJob.JobId);
        }

        player.EmitLocked("definedjob:openmenu", _gameOptions.DefinedJobs, playerJob);
    }

    public async Task UpdateUi(ServerPlayer player)
    {
        var definedJob = await GetPlayerJob(player);
        DefinedJobData? playerJob = null;
        if (definedJob != null)
        {
            playerJob = _gameOptions.DefinedJobs.Find(j => j.Id == definedJob.JobId);
        }

        lock (player)
        {
            player.EmitGui("jobmenu:sendplayerjob", playerJob);
        }
    }
}