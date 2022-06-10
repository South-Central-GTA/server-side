using System;
using System.Threading.Tasks;
using AltV.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Configuration;
using Server.Core.Extensions;
using Server.Core.ScheduledJobs;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Modules.Group;
using Server.Modules.Mail;
using Server.Modules.Phone;

namespace Server.ScheduledJobs;

public class CompanyScheduledJob : ScheduledJob
{
    private readonly GroupModule _groupModule;
    private readonly GroupService _groupService;
    private readonly ILogger<CompanyScheduledJob> _logger;
    private readonly MailModule _mailModule;

    private readonly PhoneModule _phoneModule;

    public CompanyScheduledJob(ILogger<CompanyScheduledJob> logger, IOptions<GameOptions> gameOptions,
        GroupService groupService, PhoneModule phoneModule, GroupModule groupModule, MailModule mailModule) : base(
        TimeSpan.FromMinutes(gameOptions.Value.CompanyMinutesInterval))
    {
        _logger = logger;

        _groupService = groupService;

        _phoneModule = phoneModule;
        _groupModule = groupModule;
        _mailModule = mailModule;
    }

    public override async Task Action()
    {
        var groups = await _groupService.Where(b => b.Status == GroupState.REQUESTED);

        // We have to toggle the state here and save it to the database because we need it to update it on ui side.
        groups.ForEach(b => b.Status = GroupState.CREATED);

        await _groupService.UpdateRange(groups);

        foreach (var group in groups)
        {
            var member = group.Members.Find(m => m.Owner);
            if (member != null)
            {
                var player = Alt.GetAllPlayers().FindPlayerByCharacterId(member.CharacterModelId);
                if (player != null)
                {
                    var phone = await _phoneModule.GetByOwner(player.CharacterModel.Id);
                    if (phone != null)
                    {
                        await _phoneModule.SendNotification(phone.Id, PhoneNotificationType.GOV,
                            $"Das angemeldete Unternehmen {group.Name}, wurde erfolgreich in unserem Register aufgenommen und freigeschaltet.");

                        await _groupModule.UpdateUi(player);
                        await _mailModule.UpdateUi(player);
                    }
                }
            }
        }

        _logger.LogInformation($"{groups.Count} companies got activated.");
        await Task.CompletedTask;
    }
}