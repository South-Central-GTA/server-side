using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Timers;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Elements.Entities;
using Server.Data.Models;
using Server.Database.Enums;
using Server.Database.Models;
using Server.Database.Models.Character;
using Server.Database.Models.Inventory;

namespace Server.Core.Entities;

public class ServerPlayer
    : Player
{
    private AccountModel _accountModel;
    private bool _adminFreezed;
    private CharacterModel _characterModel;
    private bool _isAduty;
    private bool _isInFreeCam;
    public ulong DiscordId;
    public bool IsSpawned;

    public ServerPlayer(IServer server, IntPtr entityPointer, ushort id)
        : base(server, entityPointer, id)
    {
    }
    
    public string AccountName => AccountModel.CurrentName;

    public AccountModel AccountModel
    {
        get => _accountModel;
        set
        {
            _accountModel = value;
            this.EmitLocked("account:sync", _accountModel);
        }
    }

    public CharacterModel CharacterModel
    {
        get => _characterModel;
        set
        {
            _characterModel = value;
            this.EmitLocked("character:sync", _characterModel);
        }
    }

    public bool AdminFreezed
    {
        get => _adminFreezed;
        set
        {
            _adminFreezed = value;
            this.EmitLocked("player:adminfreeze", _adminFreezed);
        }
    }

    public ConcurrentDictionary<string, Timer> Timers { get; set; } = new();

    public bool IsLoggedIn => IsConnected && AccountModel != null;
    
    public int LoginTrys { get; set; }
    
    public PhoneCallData? PhoneCallData { get; set; }

    public List<InventoryModel> DefaultInventories { get; set; } = new();

    public bool IsAduty
    {
        get => _isAduty;
        set
        {
            _isAduty = value;
            Invincible = value;
            this.EmitLocked("player:setaduty", value);
        }
    }

    public bool IsDuty { get; set; }
    
    public int DutyLeaseCompanyHouseId { get; set; }
    
    public bool IsInventoryOpen { get; set; }
    
    public bool IsPhoneOpen { get; set; }
    
    public bool IsInFreeCam
    {
        get => _isInFreeCam;
        set
        {
            _isInFreeCam = value;
            Visible = !value;
        }
    }
    
    public bool IsInBigEars { get; set; }

    public bool Cuffed
    {
        get => _cuffed;
        set
        {
            if (value)
            {
                this.EmitLocked("player:cuff");
            }
            else
            {
                this.EmitLocked("player:uncuff");
            }

            _cuffed = value;
        }
    }

    private bool _cuffed;

    public uint MloInterior { get; set; }
}