using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Server.Database.Models._Base;

public abstract class ModelBase
{
    protected ModelBase()
    {
        CreatedAt = DateTime.Now;
        LastUsage = DateTime.Now;
    }

    [JsonIgnore] 
    public DateTime CreatedAt { get; set; }

    [JsonIgnore] 
    public DateTime LastUsage { get; set; }
}