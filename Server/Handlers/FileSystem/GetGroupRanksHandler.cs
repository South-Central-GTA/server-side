using System;
using System.Collections.Generic;
using System.Security.Principal;
using AltV.Net;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Group;

namespace Server.Handlers.FileSystem;

public class GetGroupRanksHandler : ISingletonScript
{
    private readonly GroupRankService _groupRankService;
    private readonly DirectoryService _directoryService;
    
    public GetGroupRanksHandler(
        GroupRankService groupRankService, 
        DirectoryService directoryService)
    {
        _groupRankService = groupRankService;
        _directoryService = directoryService;

        AltAsync.OnClient<ServerPlayer, int, int>("filesystem:getranksetup", OnExecuteEvent);
    }

    private async void OnExecuteEvent(ServerPlayer player, int groupId, int directoryId)
    {
        if (!player.Exists)
        {
            return;
        }

        var groupRanks = await _groupRankService.Where(gr => gr.GroupModelId == groupId);

        var directory = await _directoryService.GetByKey(directoryId);
        if (directory == null)
        {
            return;
        }        
        
        player.EmitGui("filesystem:getranksetup", new FileSystemRankSetup()
        {
            CanReadLevel = directory.ReadGroupLevel,
            CanWriteLevel = directory.WriteGroupLevel,
            Ranks = groupRanks
        });
    }

    private struct FileSystemRankSetup : IWritable
    {
        public int CanReadLevel { get; set; }
        public int CanWriteLevel { get; set; }
        public List<GroupRankModel> Ranks { get; set; }
        
        public void OnWrite(IMValueWriter writer)
        {
            writer.BeginObject();

            writer.Name("canReadLevel");
            writer.Value(CanReadLevel);
            
            writer.Name("canWriteLevel");
            writer.Value(CanWriteLevel);
            
            writer.Name("ranks");
            writer.BeginArray();

            foreach (var t in Ranks)
            {
                writer.BeginObject();

                var rank = t;

                writer.Name("groupId");
                writer.Value(rank.GroupModelId);

                writer.Name("level");
                writer.Value(rank.Level);

                writer.Name("name");
                writer.Value(rank.Name);

                writer.Name("groupPermission");
                writer.Value((int)rank.GroupPermission);

                writer.EndObject();
            }

            writer.EndArray();
            
            writer.EndObject();
        }
    }
}