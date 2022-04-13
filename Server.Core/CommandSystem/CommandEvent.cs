using System;

namespace Server.Core.CommandSystem;

[AttributeUsage(AttributeTargets.Method)]
public class CommandEvent
    : Attribute
{
    public CommandEvent(CommandEventType eventType)
    {
        EventType = eventType;
    }

    public CommandEventType EventType { get; }
}