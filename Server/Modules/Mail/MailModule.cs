using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AltV.Net;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Character;
using Server.Database.Models.Group;
using Server.Database.Models.Mail;

namespace Server.Modules.Mail;

public class MailModule
    : ITransientScript
{
    private readonly GroupService _groupService;
    private readonly MailAccountCharacterAccessService _mailAccountCharacterAccessService;
    private readonly MailAccountService _mailAccountService;
    private readonly MailService _mailService;

    public MailModule(
        MailAccountService mailAccountService,
        MailService mailService,
        MailAccountCharacterAccessService mailAccountCharacterAccessService,
        GroupService groupService)
    {
        _mailAccountService = mailAccountService;
        _mailService = mailService;
        _mailAccountCharacterAccessService = mailAccountCharacterAccessService;
        _groupService = groupService;
    }

    public async Task UpdateUi(ServerPlayer player)
    {
        var mailAccounts = await _mailAccountService.GetByCharacter(player.CharacterModel.Id);
        var groups = await _groupService.Where(g => g.Members.Any(m => m.CharacterModelId == player.CharacterModel.Id));

        foreach (var group in groups)
        {
            var member = group.Members.FirstOrDefault(m => m.CharacterModelId == player.CharacterModel.Id);
            if (member == null)
            {
                continue;
            }

            if (!member.Owner)
            {
                var rank = group.Ranks.Find(r => r.Level == member.RankLevel);
                if (rank == null
                    || !rank.GroupPermission.HasFlag(GroupPermission.MAILING_SENDING)
                    && !rank.GroupPermission.HasFlag(GroupPermission.MAILING_READING)
                    && !rank.GroupPermission.HasFlag(GroupPermission.MAILING_DELETING))
                {
                    continue;
                }
            }

            mailAccounts.Add(await _mailAccountService.GetByGroup(group.Id));
        }

        player.EmitGui("mailing:updatemailaccounts", mailAccounts);
    }

    public async Task CreateMailAccount(ServerPlayer player, string mailAddress)
    {
        if (!player.Exists)
        {
            return;
        }

        mailAddress = mailAddress.ToLower();

        var mailAccount = await _mailAccountService.GetByKey(mailAddress);
        if (mailAccount != null)
        {
            player.EmitGui("laptop:showerrormessage", "Diese Mail Adresse ist schon vergeben.");
            return;
        }

        await _mailAccountService.Add(new MailAccountModel
        {
            Type = OwnableAccountType.PRIVATE,
            MailAddress = mailAddress,
            CharacterAccesses = new List<MailAccountCharacterAccessModel>
            {
                new()
                {
                    MailAccountModelMailAddress = mailAddress,
                    CharacterModelId = player.CharacterModel.Id,
                    Permission = MailingPermission.NONE,
                    Owner = true
                }
            },
            GroupAccess = new List<MailAccountGroupAccessModel>()
        });

        await UpdateUi(player);
    }

    public async Task CreateMailAccount(GroupModel groupModel)
    {
        var mailAddress = groupModel.Name.ToLower();
        mailAddress = Regex.Replace(mailAddress, @"\s+", "");
        await _mailAccountService.Add(new MailAccountModel
        {
            Type = OwnableAccountType.GROUP,
            MailAddress = mailAddress,
            GroupAccess = new List<MailAccountGroupAccessModel>
            {
                new()
                {
                    MailAccountModelMailAddress = mailAddress,
                    GroupModelId = groupModel.Id,
                    Owner = true
                }
            },
            CharacterAccesses = new List<MailAccountCharacterAccessModel>()
        });
    }

    public async Task<bool> HasPermission(ServerPlayer player, MailAccountModel mailAccountModel,
                                          MailingPermission mailingPermission)
    {
        var hasAccess = false;

        var characterAccess =
            mailAccountModel.CharacterAccesses.Find(ca => ca.CharacterModelId == player.CharacterModel.Id);
        if (characterAccess == null)
        {
            var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    var bankAccountGroupAccess = mailAccountModel.GroupAccess.Find(ga => ga.GroupModelId == group.Id);
                    if (bankAccountGroupAccess != null)
                    {
                        var groupMember = group.Members.Find(m => m.CharacterModelId == player.CharacterModel.Id);
                        if (groupMember != null)
                        {
                            if (groupMember.Owner)
                            {
                                hasAccess = true;
                            }

                            // Dirty but it should work..
                            var groupPermission = GroupPermission.NONE;

                            if (mailingPermission == MailingPermission.SENDING)
                            {
                                groupPermission = GroupPermission.MAILING_SENDING;
                            }

                            if (mailingPermission == MailingPermission.READING)
                            {
                                groupPermission = GroupPermission.MAILING_READING;
                            }

                            if (mailingPermission == MailingPermission.DELETING)
                            {
                                groupPermission = GroupPermission.MAILING_DELETING;
                            }

                            var rank = group.Ranks.Find(r => r.Level == groupMember.RankLevel);
                            if (rank != null && rank.GroupPermission.HasFlag(groupPermission))
                            {
                                hasAccess = true;
                            }
                        }
                    }
                }
            }
        }
        else
        {
            if (characterAccess.Permission.HasFlag(mailingPermission) || characterAccess.Owner)
            {
                hasAccess = true;
            }
        }

        return hasAccess;
    }

    public async Task<bool> IsOwner(ServerPlayer player, MailAccountModel mailAccountModel)
    {
        var isOwner = false;

        var characterAccess =
            mailAccountModel.CharacterAccesses.Find(ca => ca.CharacterModelId == player.CharacterModel.Id);
        if (characterAccess == null)
        {
            var groups = await _groupService.GetGroupsByCharacter(player.CharacterModel.Id);
            if (groups != null)
            {
                foreach (var group in groups)
                {
                    var bankAccountGroupAccess = mailAccountModel.GroupAccess.Find(ga => ga.GroupModelId == group.Id);
                    if (bankAccountGroupAccess != null)
                    {
                        var groupMember = group.Members.Find(m => m.CharacterModelId == player.CharacterModel.Id);
                        if (groupMember is { Owner: true })
                        {
                            isOwner = true;
                        }
                    }
                }
            }
        }
        else
        {
            if (characterAccess.Owner)
            {
                isOwner = true;
            }
        }

        return isOwner;
    }

    public async Task SendMail(ServerPlayer player, string senderMailAddress, string targetMailAddress,
                               string title, string context)
    {
        if (!player.Exists)
        {
            return;
        }

        if (senderMailAddress.ToLower() == targetMailAddress.ToLower())
        {
            player.EmitGui("mail:senderror", "Sie können keine Mail auf das eigene Mailkonto schicken.");
            player.EmitGui("mail:sendbackup", title, context);
            return;
        }

        var mailAccount = await _mailAccountService.GetByKey(senderMailAddress.ToLower());
        if (mailAccount == null)
        {
            return;
        }

        var targetMailAccount = await _mailAccountService.GetByKey(targetMailAddress.ToLower());
        if (targetMailAccount == null)
        {
            player.EmitGui("mail:senderror",
                           "Die angegebene Mail wurde bei uns nicht gefunden, die Mail konnte nicht verschickt werden.");
            player.EmitGui("mail:sendbackup", title, context);
            return;
        }

        if (!await HasPermission(player, mailAccount, MailingPermission.SENDING))
        {
            return;
        }

        var newMail = new MailModel
        {
            Title = title,
            Context = context,
            SenderMailAddress = senderMailAddress,
            MailReadedFromAddress = new List<string>(),
            MailLinks = new List<MailLinkModel>
            {
                new() { MailAccountModelMailAddress = senderMailAddress, IsAuthor = true },
                new() { MailAccountModelMailAddress = targetMailAddress, IsAuthor = false }
            }
        };

        await _mailService.Add(newMail);

        await UpdatePlayersUi(targetMailAccount);
        await UpdatePlayersUi(mailAccount);

        player.EmitGui("mail:sendinfo", $"Mail wurde erfolgreich verschickt.");
    }

    public async Task DeleteMail(string mailAddress, int mailId)
    {
        var mailAccount = await _mailAccountService.GetByKey(mailAddress);
        if (mailAccount == null)
        {
            return;
        }

        var mail = await _mailService.GetByKey(mailId);
        await _mailService.Remove(mail);

        await UpdatePlayersUi(mailAccount);
    }

    private async Task UpdatePlayersUi(MailAccountModel targetMailAccountModel)
    {
        foreach (var bankAccountCharacterAccess in targetMailAccountModel.CharacterAccesses)
        {
            var serverPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(bankAccountCharacterAccess.CharacterModelId);
            if (serverPlayer != null)
            {
                await UpdateUi(serverPlayer);
            }
        }

        foreach (var groupAccess in targetMailAccountModel.GroupAccess)
        {
            var group = await _groupService.GetByKey(groupAccess.GroupModelId);

            foreach (var member in group.Members)
            {
                if (member.Owner)
                {
                    var ownerPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(member.CharacterModelId);
                    if (ownerPlayer != null)
                    {
                        await UpdateUi(ownerPlayer);
                        return;
                    }
                }

                var rank = group.Ranks.Find(r => r.Level == member.RankLevel);
                if (rank == null || !rank.GroupPermission.HasFlag(GroupPermission.MAILING_SENDING)
                    && !rank.GroupPermission.HasFlag(GroupPermission.MAILING_READING)
                    && !rank.GroupPermission.HasFlag(GroupPermission.MAILING_DELETING))
                {
                    continue;
                }

                var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(member.CharacterModelId);
                if (targetPlayer != null)
                {
                    await UpdateUi(targetPlayer);
                }
            }
        }
    }

    public async Task AddCharacterToAccount(ServerPlayer player, CharacterModel characterModel,
                                            MailAccountModel mailAccountModel)
    {
        await _mailAccountCharacterAccessService.Add(new MailAccountCharacterAccessModel
        {
            CharacterModelId = characterModel.Id,
            MailAccountModelMailAddress = mailAccountModel.MailAddress
        });

        await UpdateUi(player);

        var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(characterModel.Id);
        if (targetPlayer != null)
        {
            await UpdateUi(targetPlayer);
        }

        player.EmitGui("mail:sendinfo",
                       $"Wir haben erfolgreich {characterModel.Name} auf Ihr Mailkonto freigeschaltet, richten Sie nun bitte die Berechtigungen in der App ein.");
    }


    public async Task RemoveCharacterFromAccount(ServerPlayer player, CharacterModel characterModel,
                                                 MailAccountCharacterAccessModel mailAccountCharacterAccessModel)
    {
        await _mailAccountCharacterAccessService.Remove(mailAccountCharacterAccessModel);

        await UpdateUi(player);

        var targetPlayer = Alt.GetAllPlayers().FindPlayerByCharacterId(characterModel.Id);
        if (targetPlayer != null)
        {
            await UpdateUi(targetPlayer);
        }

        player.EmitGui("mail:sendinfo", $"Wir haben {characterModel.Name} von Ihrem Mailkonto entfernt.");
    }

    public async Task<bool> AddPermission(MailAccountModel mailAccountModel, int characterId,
                                          MailingPermission mailingPermission)
    {
        var characterAccess = await _mailAccountCharacterAccessService.Find(ca =>
                                                                                ca.MailAccountModelMailAddress ==
                                                                                mailAccountModel.MailAddress &&
                                                                                ca.CharacterModelId == characterId);
        if (characterAccess == null)
        {
            return false;
        }

        characterAccess.Permission |= mailingPermission;

        await _mailAccountCharacterAccessService.Update(characterAccess);
        return true;
    }

    public async Task<bool> RemovePermission(MailAccountModel mailAccountModel, int characterId,
                                             MailingPermission mailingPermission)
    {
        var characterAccess = await _mailAccountCharacterAccessService.Find(ca =>
                                                                                ca.MailAccountModelMailAddress ==
                                                                                mailAccountModel.MailAddress &&
                                                                                ca.CharacterModelId == characterId);
        if (characterAccess == null)
        {
            return false;
        }

        characterAccess.Permission &= ~mailingPermission;

        await _mailAccountCharacterAccessService.Update(characterAccess);
        return true;
    }
}