using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Modules.DrivingSchool;

namespace Server.Handlers.DrivingSchool;

public class StopDrivingSchoolHandler : ISingletonScript
{
    private readonly DrivingSchoolModule _drivingSchoolModule;

    public StopDrivingSchoolHandler(DrivingSchoolModule drivingSchoolModule)
    {
        _drivingSchoolModule = drivingSchoolModule;

        AltAsync.OnClient<ServerPlayer>("drivingschool:forcestop", OnForceStop);
    }

    private async void OnForceStop(ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        player.SendNotification("Du bist zu weit von dem nächsten Checkpoint entfernt.", NotificationType.ERROR);

        await _drivingSchoolModule.StopPlayerExam(player, false);
    }
}