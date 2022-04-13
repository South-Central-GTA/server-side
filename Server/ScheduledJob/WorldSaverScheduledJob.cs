// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Logging;
// using Microsoft.Extensions.Options;
// using Server.Core.Abstractions;
// using Server.Core.Configuration;
//
// namespace Server.ScheduledJob
// {
//     public class WorldSaverScheduledJob
//         : ScheduledJob
//     {
//         private readonly IEnumerable<IServerJob> _serverJobs;
//         private readonly ILogger<WorldSaverScheduledJob> _logger;
//
//         public WorldSaverScheduledJob(
//             ILogger<WorldSaverScheduledJob> logger,
//             IOptions<GameOptions> gameOptions,
//             IEnumerable<IServerJob> serverJobs)
//             : base(TimeSpan.FromSeconds(gameOptions.Value.SaveInterval))
//         {
//             _logger = logger;
//             _serverJobs = serverJobs;
//         }
//
//         public override async Task Action()
//         {
//             _logger.LogInformation($"World save initiated at {DateTime.Now}");
//
//             // execute save method of all server jobs
//             var taskList = new List<Task>();
//             Parallel.ForEach(_serverJobs, job => taskList.Add(job.OnSave()));
//
//             // wait until all jobs finished
//             await Task.WhenAll(taskList.ToArray());
//
//             _logger.LogInformation($"World save completed at {DateTime.Now}");
//             
//             await Task.CompletedTask;
//         }
//     }
// }

