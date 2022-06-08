using System;
using System.Threading.Tasks;

namespace Server.Core.ScheduledJobs;

public abstract class ScheduledJob
{
    public ScheduledJob(TimeSpan interval)
    {
        Interval = interval;

        Id = Guid.NewGuid().ToString();
        LastExecution = DateTime.MinValue;
    }

    public string Id { get; }

    public TimeSpan Interval { get; }

    public DateTime LastExecution { get; set; }

    public abstract Task Action();
}