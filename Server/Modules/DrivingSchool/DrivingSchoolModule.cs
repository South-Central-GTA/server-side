using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.Data.Enums;
using Server.Data.Models;
using Server.DataAccessLayer.Services;
using Server.Database.Enums;
using Server.Database.Models.Character;
using Server.Modules.Chat;
using Server.Modules.EntitySync;
using Server.Modules.Inventory;
using Server.Modules.Vehicles;

namespace Server.Modules.DrivingSchool;

public class DrivingSchoolModule
    : ITransientScript
{
    private readonly CharacterService _characterService;
    private readonly PersonalLicenseService _personalLicenseService;
    private readonly ChatModule _chatModule;
    private readonly ItemCreationModule _itemCreationModule;
    private readonly ILogger<DrivingSchoolModule> _logger;
    private readonly PedSyncModule _pedSyncModule;

    private readonly Random _random = new();


    private readonly List<string> _sentences = new()
    {
        "Also dann fangen wir mal mit der Prüfung an, fahre bitte die Maximalgeschwindigkeit innerorts, ich möchte fertig werden und nicht trödeln! Ich würde über die Fahrt bisschen was erzählen, einfach damit die Spannung nen bisschen abfällt. Bitte links.",
        "Hier links direkt neben der Davis Police Station ist der Impound Lot des LSPD. Hier landen falsch geparkte Fahrzeuge, die dann von den Besitzern wieder abgeholt werden müssen. Das solltest du unbedingt vermeiden, die Kosten sind enorm und der bürokratische Aufwand ist groß! Links bitte.",
        "Hier ist die Davis Police Station, bis vor kurzem noch Sheriff Station. Du hast es bestimmt schon einmal gehört aber hier wurde Scheiße gebaut. So große Scheiße, dass der Sheriff abgezogen wurde. *lacht* und das muss man erstmal schaffen.",
        "Der Leimert Park, ein toller Ort hier im Viertel. Hier kann man entspannen und die Seele baumeln lassen. Ab und wann werden hier auch Snacks und Getränke verkauft. Bist du eigentlich Neu hier in der Gegend? Nah- egal Augen auf die Straße!",
        "Wir fahren geradeaus an der Stadthalle und dem Krankenhaus vorbei. In der Stadthalle ist das Büro des Bezirksbürgermeisters.",
        "Hier folgen wir der Straße nach links, also einfach in der Spur bleiben. Dort ist übrigens meine top Adresse um sich neue Shirts und Hosen zu kaufen - genau links. Der Laden ist klasse und das Personal hat richtig Ahnung.",
        "Ah Chamberlain Hills. Hier bin ich aufgewachsen, ist ein typisches Arbeiterviertel mit seinen typischen Problemen - immerhin kann man hier günstig wohnen. In Los Santos ist das ja eher eine Seltenheit geworden. Achte bitte etwas auf deine Spur ja, hier fahren auch andere.. Links rum!",
        "Fahre erstmal gerade aus bis zur nächsten Kreuzung runter, dann die nächste rechts.",
        "Jetzt rechts bitte, achte auf deinen Schulterblick. Radfahrer würde sich zu gern das Schmerzensgeld abholen.",
        "Hier an der großen Kreuzung bitte nach links in die Grove Street rein gleich an der Tankstelle vorbei. LTD ist der größte Mineralölkonzern in ganz San Andreas! Hier bei der Tankstelle kann man eigentlich alles Mögliche einkaufen, schon beängstigend, was die für Macht haben.",
        "Gerade zu ist die Grove Street, bekannt für seine Gang Affinität. Da wollen wir lieber nicht reinplatzen, deren- nennen wir es Straßenfeste sind immer recht interessant. Rechts befindet sich eine Garage zum sicheren Verwahren deines zukünftigen Fahrzeuges. Eine von vielen Public Garages in ganz Los Santos.",
        "Jetzt bitte links die Gasse rein.",
        "Gerade zu ist die Blue Mall, bekannt im ganzen Stadtteil. Der riesige Parkplatz wird von so ziemlich jedem hier genutzt. Ab und an soll es hier aber Autodiebstähle geben, also aufpassen! Wir fahren nach links weiter.",
        "Achte etwas auf die Vorfahrtsregeln, wenn du von einem abgesenkten Bordstein kommst musst du warten! Hier bitte rechts rum zur Metro Station.",
        "Hier bitte wieder rechts, genau an der Metro Station vorbei. Die Station verbindet Davis mit dem Rest von Los Santos. Die öffentliche Anbindung hier ist richtig gut finde ich.",
        "Also mir hat die Fahrt gereicht, fahre bitte zurück zur Fahrschule, dann sag ich dir dein Ergebnis..."
    };

    private readonly VehicleCatalogService _vehicleCatalogService;

    private readonly VehicleModule _vehicleModule;

    private readonly WorldLocationOptions _worldLocationOptions;

    public DrivingSchoolModule(
        ILogger<DrivingSchoolModule> logger,
        IOptions<WorldLocationOptions> worldLocationOptions,
        CharacterService characterService,
        VehicleCatalogService vehicleCatalogService,
        PersonalLicenseService personalLicenseService,
        VehicleModule vehicleModule,
        PedSyncModule pedSyncModule,
        ChatModule chatModule,
        ItemCreationModule itemCreationModule)
    {
        _logger = logger;

        _worldLocationOptions = worldLocationOptions.Value;

        _characterService = characterService;
        _vehicleCatalogService = vehicleCatalogService;

        _vehicleModule = vehicleModule;
        _pedSyncModule = pedSyncModule;
        _chatModule = chatModule;
        _itemCreationModule = itemCreationModule;
        _personalLicenseService = personalLicenseService;
    }

    public async Task SetPlayerInExam(DrivingSchoolData drivingSchoolData, ServerPlayer player)
    {
        if (!player.Exists)
        {
            return;
        }

        var randomModel = drivingSchoolData.VehicleModels[_random.Next(drivingSchoolData.VehicleModels.Length)];
        var catalogVehicle = await _vehicleCatalogService.GetByKey(randomModel);
        if (catalogVehicle == null)
        {
            return;
        }

        var vehicle = await _vehicleModule.Create(randomModel,
                                                  new Position(drivingSchoolData.StartPointX,
                                                               drivingSchoolData.StartPointY,
                                                               drivingSchoolData.StartPointZ),
                                                  new DegreeRotation(drivingSchoolData.StartPointRoll,
                                                                     drivingSchoolData.StartPointPitch,
                                                                     drivingSchoolData.StartPointYaw),
                                                  drivingSchoolData.VehPrimColor,
                                                  drivingSchoolData.VehSecColor,
                                                  0,
                                                  1000,
                                                  1000,
                                                  catalogVehicle.MaxTank);

        if (vehicle == null)
        {
            return;
        }

        vehicle.NumberplateText = await _vehicleModule.GetRandomNumberplate();

        _pedSyncModule.CreateTemp(player, "cs_barry", player.Position, 0, player.Dimension, vehicle);

        vehicle.SetData("DRIVING_SCHOOL_CHARACTER_ID", player.CharacterModel.Id);
        vehicle.SetData("DRIVING_SCHOOL_ID", drivingSchoolData.Id);

        player.SetData("DRIVING_SCHOOL_VEH", vehicle);

        await player.SetIntoVehicleAsync(vehicle, 1);

        player.EmitLocked("drivingschool:start",
                          drivingSchoolData.StartPointX,
                          drivingSchoolData.StartPointY,
                          drivingSchoolData.StartPointZ - 0.5f);
    }

    public async Task StopPlayerExam(ServerPlayer player, bool passed)
    {
        if (!player.Exists)
        {
            return;
        }

        _pedSyncModule.Delete(player);

        player.EmitLocked("drivingschool:stop");
        player.DeleteData("DRIVING_SCHOOL_VEH");

        if (ExistExamVehicleForPlayer(player, out var vehicle))
        {
            if (vehicle != null)
            {
                await StopVehicleExam(vehicle);
            }
        }

        if (passed)
        {
            await _personalLicenseService.Add(new PersonalLicenseModel()
            {
                CharacterModelId = player.CharacterModel.Id,
                Type = PersonalLicensesType.DRIVING
            });

            await _itemCreationModule.AddItemAsync(player, ItemCatalogIds.LICENSES, 1);

            player.SendNotification("Dein Charakter hat die Prüfung bestanden.", NotificationType.SUCCESS);
        }
        else
        {
            player.SendNotification("Dein Charakter hat die Prüfung leider nicht bestanden.", NotificationType.ERROR);
        }
    }

    public void RestartPlayerExam(ServerPlayer player, ServerVehicle vehicle)
    {
        vehicle.GetData("DRIVING_SCHOOL_VEH_CHECKPOINT_INDEX", out int checkpointIndex);
        vehicle.GetData("DRIVING_SCHOOL_ID", out int drivingSchoolId);

        var drivingSchoolData = _worldLocationOptions.DrivingSchools.Find(g => g.Id == drivingSchoolId);
        if (drivingSchoolData == null)
        {
            return;
        }

        player.EmitLocked("drivingschool:restart",
                          drivingSchoolData.StartPointX,
                          drivingSchoolData.StartPointY,
                          drivingSchoolData.StartPointZ - 0.5f,
                          checkpointIndex);
    }

    public async Task StopVehicleExam(ServerVehicle vehicle)
    {
        vehicle.DeleteData("DRIVING_SCHOOL_CHARACTER_ID");
        vehicle.DeleteData("DRIVING_SCHOOL_ID");
        vehicle.DeleteData("DRIVING_SCHOOL_VEH_CHECKPOINT_INDEX");

        await vehicle.RemoveAsync();
    }

    public void ReportSpeeding(ServerPlayer player)
    {
        _chatModule.SendProxMessage("Fahrlehrer",
                                    5,
                                    ChatType.SPEAK,
                                    "Das ist hier kein Rennen! Halte dich an die maximale Geschwindigkeit innerhalb geschlossener Ortschaften! Das sind 70 km/h!",
                                    player.Vehicle.Position,
                                    0);

        player.EmitLocked("drivingschool:resetreportspeeding");
    }

    public bool ExistExamVehicleForPlayer(ServerPlayer player, out ServerVehicle? drivingSchoolVehicle)
    {
        foreach (var vehicle in Alt.GetAllVehicles().Cast<ServerVehicle>())
        {
            if (!vehicle.HasData("DRIVING_SCHOOL_CHARACTER_ID"))
            {
                continue;
            }

            vehicle.GetData("DRIVING_SCHOOL_CHARACTER_ID", out int characterId);
            if (characterId != player.CharacterModel.Id)
            {
                continue;
            }

            drivingSchoolVehicle = vehicle;
            return true;
        }

        drivingSchoolVehicle = null;
        return false;
    }

    public async void RequestNextCheckpoint(ServerPlayer player, int checkpointIndex, bool isLastCheckpoint)
    {
        if (!player.Exists)
        {
            return;
        }

        player.GetData("DRIVING_SCHOOL_VEH", out ServerVehicle serverVehicle);
        if (!player.IsInVehicle)
        {
            await StopPlayerExam(player, false);
            player.SendNotification("Du hast das Fahrschulauto zurückgelassen und somit die Prüfung nicht bestanden.",
                                    NotificationType.ERROR);
            return;
        }

        if (!Equals(player.Vehicle, serverVehicle))
        {
            await StopPlayerExam(player, false);
            player.SendNotification("Du hast das Fahrschulauto zurückgelassen und somit die Prüfung nicht bestanden.",
                                    NotificationType.ERROR);
            return;
        }

        if (isLastCheckpoint)
        {
            _chatModule.SendProxMessage("Fahrlehrer",
                                        5,
                                        ChatType.SPEAK,
                                        "Alles klar dann haben wirs...",
                                        player.Vehicle.Position,
                                        0);

            await StopPlayerExam(player, true);
        }
        else
        {
            _chatModule.SendProxMessage("Fahrlehrer",
                                        5,
                                        ChatType.SPEAK,
                                        _sentences[checkpointIndex],
                                        player.Vehicle.Position,
                                        0);
        }

        serverVehicle.SetData("DRIVING_SCHOOL_VEH_CHECKPOINT_INDEX", checkpointIndex + 1);

        player.EmitLocked("drivingschool:sendnextcheckpoint");
    }
}