using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Configuration;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules;
using Server.Modules.World;

namespace Server.ScheduledJob;

public class EmergencyCallsScheduledJob
    : ScheduledJob
{
    private readonly ILogger<EmergencyCallsScheduledJob> _logger;
    private readonly EmergencyCallService _emergencyCallService;
    private readonly MdcOptions _mdcOptions;
    
    public EmergencyCallsScheduledJob(
        ILogger<EmergencyCallsScheduledJob> logger, 
        EmergencyCallService emergencyCallService, 
        IOptions<MdcOptions> mdcOptions)
        : base(TimeSpan.FromMinutes(1))
    {
        _logger = logger;
        _emergencyCallService = emergencyCallService;
        _mdcOptions = mdcOptions.Value;
    }

    public override async Task Action()
    {
        var emergencyCalls = await _emergencyCallService.GetAll();

        var now = DateTime.Now;
        await _emergencyCallService.RemoveRange(emergencyCalls.Where(call => (now - call.CreatedAt).TotalMinutes >= _mdcOptions.EmergencyCallMinutesLifetime));
        
        await Task.CompletedTask;
    }
}