using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using AltV.Net;
using Server.Database.Enums;
using Server.Database.Models._Base;

namespace Server.Database.Models.Vehicles;

public class CatalogVehicleModel : ModelBase, IWritable
{
    [Key]
    [JsonPropertyName("model")]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public string Model { get; set; }

    [JsonPropertyName("displayName")] public string DisplayName { get; set; }

    [JsonPropertyName("displayClass")] public string DisplayClass { get; set; }

    [JsonPropertyName("classId")] public string ClassId { get; set; }

    [JsonPropertyName("maxTank")] public int MaxTank { get; set; }

    [JsonPropertyName("fuelType")] public FuelType FuelType { get; set; }

    [JsonPropertyName("price")] public int Price { get; set; }

    [JsonPropertyName("dlcName")] public string DlcName { get; set; }

    /// <summary>
    ///     Value has to be set before using on client side. Use points module to calculate the points.
    /// </summary>
    [NotMapped]
    public int SouthCentralPoints { get; set; }

    public int AmountOfOrderableVehicles { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        Serialize(this, writer);
    }

    public static void Serialize(CatalogVehicleModel model, IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("model");
        writer.Value(model.Model);

        writer.Name("displayName");
        writer.Value(model.DisplayName);

        writer.Name("displayClass");
        writer.Value(model.DisplayClass);

        writer.Name("classId");
        writer.Value(model.ClassId);

        writer.Name("maxTank");
        writer.Value(model.MaxTank);

        writer.Name("fuelType");
        writer.Value((int)model.FuelType);

        writer.Name("price");
        writer.Value(model.Price);

        writer.Name("southCentralPoints");
        writer.Value(model.SouthCentralPoints);

        writer.Name("dlcName");
        writer.Value(model.DlcName);

        writer.Name("amountOfOrderableVehicles");
        writer.Value(model.AmountOfOrderableVehicles);

        writer.EndObject();
    }
}