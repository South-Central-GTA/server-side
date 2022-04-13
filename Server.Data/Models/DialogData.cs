using System.Text.Json;
using AltV.Net;
using Server.Data.Enums;

namespace Server.Data.Models;

public class DialogData
    : IWritable
{
    public DialogType Type { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public bool HasBankAccountSelection { get; set; }
    public bool HasInputField { get; set; }
    public bool FreezeGameControls { get; set; }
    public object[] Data { get; set; }
    public string PrimaryButton { get; set; }
    public string SecondaryButton { get; set; }
    public string PrimaryButtonServerEvent { get; set; }
    public string SecondaryButtonServerEvent { get; set; }
    public string CloseButtonServerEvent { get; set; }
    public string PrimaryButtonClientEvent { get; set; }
    public string SecondaryButtonClientEvent { get; set; }
    public string CloseButtonClientEvent { get; set; }

    public void OnWrite(IMValueWriter writer)
    {
        writer.BeginObject();

        writer.Name("type");
        writer.Value((int)Type);

        writer.Name("title");
        writer.Value(Title);

        writer.Name("description");
        writer.Value(Description);

        writer.Name("hasBankAccountSelection");
        writer.Value(HasBankAccountSelection);

        writer.Name("hasInputField");
        writer.Value(HasInputField);

        writer.Name("freezeGameControls");
        writer.Value(FreezeGameControls);

        writer.Name("dataJson");
        writer.Value(JsonSerializer.Serialize(Data));

        writer.Name("primaryButton");
        writer.Value(PrimaryButton);

        writer.Name("secondaryButton");
        writer.Value(SecondaryButton);

        writer.Name("primaryButtonServerEvent");
        writer.Value(PrimaryButtonServerEvent);

        writer.Name("secondaryButtonServerEvent");
        writer.Value(SecondaryButtonServerEvent);

        writer.Name("closeButtonServerEvent");
        writer.Value(CloseButtonServerEvent);

        writer.Name("primaryButtonClientEvent");
        writer.Value(PrimaryButtonClientEvent);

        writer.Name("secondaryButtonClientEvent");
        writer.Value(SecondaryButtonClientEvent);

        writer.Name("closeButtonClientEvent");
        writer.Value(CloseButtonClientEvent);

        writer.EndObject();
    }
}