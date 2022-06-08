﻿using System.Text.Json;
using AltV.Net;

namespace Server.Database.Models.Inventory;

public class ItemPoliceTicketModel
    : ItemModel, IWritable
{
    public string ReferenceId { get; set; }
    public string Reason { get; set; }
    public int Costs { get; set; }
    public bool Payed { get; set; }
    public string CreatorCharacterName { get; set; }
    public int TargetCharacterId { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("referenceId");
        writer.Value(ReferenceId);

        writer.Name("reason");
        writer.Value(Reason);

        writer.Name("costs");
        writer.Value(Costs);

        writer.Name("payed");
        writer.Value(Payed);

        writer.Name("creatorCharacterName");
        writer.Value(CreatorCharacterName);

        writer.Name("createdAtJson");
        writer.Value(JsonSerializer.Serialize(CreatedAt));

        writer.EndObject();
    }
}