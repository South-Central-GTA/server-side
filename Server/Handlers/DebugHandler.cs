using System.Globalization;
using System.IO;
using AltV.Net.Async;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;

namespace Server.Handlers;

public class DebugHandler : ISingletonScript
{
    public DebugHandler()
    {
        AltAsync.OnClient<ServerPlayer, string, float, float, float, float, float, float>("data:sendcamerainfo",
            OnSendCameraInfo);
    }

    private void OnSendCameraInfo(ServerPlayer player, string name, float x, float y, float z, float roll, float pitch,
        float yaw)
    {
        if (!player.Exists)
        {
            return;
        }

        File.AppendAllText(@"savedcampositions.txt",
            string.Format("CamPosition: new Position({0}f, {1}f, {2}f), new Rotation({3}f, {4}f, {5}f) // {6}\n",
                x.ToString().Replace(",", "."), y.ToString().Replace(",", "."), z.ToString().Replace(",", "."),
                roll.ToString().Replace(",", "."), pitch.ToString().Replace(",", "."), yaw.ToString().Replace(",", "."),
                name).ToString(new CultureInfo("en-US")));

        player.SendNotification($"Kamera Position {name} wurde erfolgreich gespeichert", NotificationType.SUCCESS);
    }
}