using System;
using AltV.Net;
using Server.Database.Enums;

namespace Server.Core.CommandSystem;

[AttributeUsage(AttributeTargets.Method)]
public class Command : Attribute, IWritable
{
    public Command(string name = null, string description = null, Permission requiredPermission = Permission.NONE,
        string[] parameterDescription = null, CommandArgs commandArgs = CommandArgs.NOT_GREEDY, string[] aliases = null)
    {
        Name = name;
        Description = description;
        RequiredPermission = requiredPermission;
        ParameterDescription = parameterDescription;
        CommandArgs = commandArgs;
        Aliases = aliases;
    }

    public string Name { get; }
    public string Description { get; }

    public Permission RequiredPermission { get; }
    public string[] ParameterDescription { get; }

    public CommandArgs CommandArgs { get; }

    public string[] Aliases { get; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("name");
        writer.Value(Name);

        writer.Name("description");
        writer.Value(Description);

        writer.Name("parameterDescription");

        writer.BeginArray();

        if (ParameterDescription != null)
        {
            for (var i = 0; i < ParameterDescription.Length; i++)
            {
                var description = ParameterDescription[i];
                writer.Value(description);
            }
        }

        writer.EndArray();

        writer.Name("greedyArg");
        writer.Value((int)CommandArgs);

        writer.Name("aliases");

        writer.BeginArray();

        if (Aliases != null)
        {
            for (var i = 0; i < Aliases.Length; i++)
            {
                var name = Aliases[i];
                writer.Value(name);
            }
        }
        else
        {
            writer.Value("[]");
        }

        writer.EndArray();

        writer.EndObject();
    }
}