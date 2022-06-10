using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.DrivingSchool;
using Server.Modules.EntitySync;

namespace Server.Handlers.DrivingSchool;

public class CheckpointDrivingSchoolHandler : ISingletonScript
{
    private readonly DrivingSchoolModule _drivingSchoolModule;
    private readonly PedSyncModule _pedSyncModule;

    public CheckpointDrivingSchoolHandler(DrivingSchoolModule drivingSchoolModule, PedSyncModule pedSyncModule)
    {
        _drivingSchoolModule = drivingSchoolModule;
        _pedSyncModule = pedSyncModule;

        AltAsync.OnClient<ServerPlayer, int, bool>("drivingschool:requestnextcheckpoint", OnRequestNextCheckpoint);
    }

    private void OnRequestNextCheckpoint(ServerPlayer player, int checkpointIndex, bool isLastCheckpoint)
    {
        if (!player.Exists)
        {
            return;
        }

        var serverPed = _pedSyncModule.GetPlayer(player);
        if (serverPed != null)
        {
            serverPed.Position = player.Position;
        }

        _drivingSchoolModule.RequestNextCheckpoint(player, checkpointIndex, isLastCheckpoint);
    }
}