using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Modules.DrivingSchool;

namespace Server.Handlers.DrivingSchool;

public class FailureChecksDrivingSchoolHandler : ISingletonScript
{
    private readonly DrivingSchoolModule _drivingSchoolModule;

    public FailureChecksDrivingSchoolHandler(DrivingSchoolModule drivingSchoolModule)
    {
        _drivingSchoolModule = drivingSchoolModule;

        AltAsync.OnClient<ServerPlayer, int>("drivingschool:reportspeeding", OnReportSpeeding);
    }

    private async void OnReportSpeeding(ServerPlayer player, int reportedSpeedings)
    {
        if (!player.Exists)
        {
            return;
        }

        if (reportedSpeedings > 3)
        {
            await _drivingSchoolModule.StopPlayerExam(player, false);
        }
        else
        {
            _drivingSchoolModule.ReportSpeeding(player);
        }
    }
}