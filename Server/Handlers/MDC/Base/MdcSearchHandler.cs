using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;

namespace Server.Handlers.MDC.Base;

public class MdcSearchHandler : ISingletonScript
{
    private readonly CharacterService _characterService;
    private readonly ItemPhoneService _itemPhoneService;
    private readonly VehicleService _vehicleService;
    private readonly BankAccountService _bankAccountService;
    private readonly MailAccountService _mailAccountService;
    private readonly GroupFactionService _groupFactionService;
    private readonly ItemWeaponService _itemWeaponService;

    public MdcSearchHandler(
        CharacterService characterService,
        ItemPhoneService itemPhoneService,
        VehicleService vehicleService,
        BankAccountService bankAccountService,
        MailAccountService mailAccountService,
        GroupFactionService groupFactionService,
        ItemWeaponService itemWeaponService)
    {
        _characterService = characterService;
        _itemPhoneService = itemPhoneService;
        _vehicleService = vehicleService;
        _bankAccountService = bankAccountService;
        _mailAccountService = mailAccountService;
        _groupFactionService = groupFactionService;
        _itemWeaponService = itemWeaponService;

        AltAsync.OnClient<ServerPlayer, string>("mdc:search", OnSearch);
    }

    private async void OnSearch(ServerPlayer player, string searchInput)
    {
        if (!player.Exists)
        {
            return;
        }

        var factionGroup = await _groupFactionService.GetFactionByCharacter(player.CharacterModel.Id);
        if (factionGroup is null)
        {
            return;
        }

        if (factionGroup.FactionType == FactionType.POLICE_DEPARTMENT)
        {
            if (Regex.Match(searchInput, @"([A-Z]{2})-(\d*)").Success)
            {
                await FindBankAccounts(player, searchInput);
                return;
            }

            if (Regex.Match(searchInput, @"^[0-9]+$").Success)
            {
                await FindPhoneNumbers(player, searchInput);
                return;
            }

            if (Regex.Match(searchInput, @"([0-9]{1})([A-Z])").Success)
            {
                await FindVehicles(player, searchInput);
                return;
            }

            if (Regex.Match(searchInput, "@mail.sa").Success)
            {
                await FindMail(player, searchInput);
                return;
            }

            if (Regex.Match(searchInput, @"([A-Z]{2})(\d*)").Success)
            {
                await FindWeapons(player, searchInput);
                return;
            }
        }

        await FindCharacterNames(player, searchInput);
    }

    private async Task FindMail(ServerPlayer player, string searchInput)
    {
        var index = searchInput.IndexOf("@", StringComparison.Ordinal);
        if (index >= 0)
        {
            searchInput = searchInput.Substring(0, index);
        }

        var mails = await _mailAccountService.Where(m => m.MailAddress.ToLower().Contains(searchInput.ToLower()));
        var mdcEntities = mails.Select(mail => new MdcSearchEntity()
        {
            Id = -1,
            StringId = mail.MailAddress,
            Name = mail.MailAddress + "@mail.sa",
            Type = MdcSearchType.MAIL
        }).Take(50).ToList();

        player.EmitGui("mdc:sendentities", mdcEntities);
    }

    private async Task FindCharacterNames(ServerPlayer player, string searchInput)
    {
        var characters =
            await _characterService.Where(c => (c.FirstName + " " + c.LastName).ToLower()
                                                                               .Contains(searchInput.ToLower()));
        var mdcEntities = characters.Select(character => new MdcSearchEntity()
        {
            Id = character.Id, Name = character.Name, Type = MdcSearchType.NAME
        }).Take(50).ToList();

        player.EmitGui("mdc:sendentities", mdcEntities);
    }

    private async Task FindPhoneNumbers(ServerPlayer player, string searchInput)
    {
        var phones = await _itemPhoneService.Where(p => p.PhoneNumber.Contains(searchInput));
        var mdcEntities = phones.Select(phone => new MdcSearchEntity()
        {
            Id = phone.Id, Name = phone.PhoneNumber, Type = MdcSearchType.NUMBER
        }).Take(50).ToList();

        player.EmitGui("mdc:sendentities", mdcEntities);
    }

    private async Task FindVehicles(ServerPlayer player, string searchInput)
    {
        var vehicles = await _vehicleService.Where(p => p.NumberplateText.ToLower().Contains(searchInput.ToLower()));
        var mdcEntities = vehicles.Select(vehicle => new MdcSearchEntity()
        {
            Id = vehicle.Id,
            Name = vehicle.NumberplateText,
            Type = MdcSearchType.VEHICLE
        }).Take(50).ToList();

        player.EmitGui("mdc:sendentities", mdcEntities);
    }

    private async Task FindBankAccounts(ServerPlayer player, string searchInput)
    {
        var bankAccounts =
            await _bankAccountService.Where(p => p.BankDetails.ToLower().Contains(searchInput.ToLower()));
        var mdcEntities = bankAccounts.Select(bankAccount => new MdcSearchEntity()
        {
            Id = bankAccount.Id,
            Name = bankAccount.BankDetails,
            Type = MdcSearchType.BANK_ACCOUNT
        }).Take(50).ToList();

        player.EmitGui("mdc:sendentities", mdcEntities);
    }

    private async Task FindWeapons(ServerPlayer player, string searchInput)
    {
        var weaponModels =
            await _itemWeaponService.Where(p => p.SerialNumber.ToLower().Contains(searchInput.ToLower()));
        var mdcEntities = weaponModels.Select(weapon => new MdcSearchEntity()
        {
            Id = weapon.Id,
            Name = weapon.SerialNumber,
            Type = MdcSearchType.WEAPON
        }).Take(50).ToList();

        player.EmitGui("mdc:sendentities", mdcEntities);
    }
}