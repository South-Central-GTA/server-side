using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.DataAccessLayer.Services;
using Server.Database.Models.Vehicles;
using Server.Modules.SouthCentralPoints;

namespace Server.Modules.Vehicles;

public class VehicleSelectionModule : ITransientScript
{
    private readonly CharacterCreatorOptions _characterCreatorOptions;
    private readonly DeliveryOptions _deliveryOptions;
    private readonly ILogger<VehicleSelectionModule> _logger;

    private readonly SouthCentralPointsModule _southCentralPointsModule;

    private readonly VehicleCatalogService _vehicleCatalogService;

    public VehicleSelectionModule(ILogger<VehicleSelectionModule> logger,
        IOptions<CharacterCreatorOptions> characterCreatorOptions, IOptions<GameOptions> gameOptions,
        IOptions<DeliveryOptions> deliveryOptions, VehicleCatalogService vehicleCatalogService,
        SouthCentralPointsModule southCentralPointsModule)
    {
        _logger = logger;

        _characterCreatorOptions = characterCreatorOptions.Value;
        _deliveryOptions = deliveryOptions.Value;

        _vehicleCatalogService = vehicleCatalogService;
        _southCentralPointsModule = southCentralPointsModule;
    }

    /// <summary>
    ///     Create a new list of catalog vehicles, added vehicles have parameters such as the price can't be zero those
    ///     vehicles are disabled,
    ///     the vehicle must be on the whitelisted classes and it is not allowed to be on the blacklisted models or blacklisted
    ///     dlcs.
    /// </summary>
    /// <returns>Starter vehicles list</returns>
    public async Task<List<CatalogVehicleModel>> GetStarterVehicles()
    {
        var catalogVehicles = await _vehicleCatalogService.GetAll();

        foreach (var catalogVehicle in catalogVehicles)
        {
            catalogVehicle.SouthCentralPoints = _southCentralPointsModule.GetPointsPrice(catalogVehicle.Price);
        }

        return catalogVehicles.FindAll(veh =>
            veh.SouthCentralPoints <= _characterCreatorOptions.MaxSouthCentralPointsVehicles && veh.Price != 0 &&
            _characterCreatorOptions.WhitelistedClasses.Contains(veh.ClassId) &&
            !_characterCreatorOptions.BlacklistedModels.Contains(veh.Model.ToLower()) &&
            !_characterCreatorOptions.BlacklistedDlcs.Contains(veh.DlcName.ToLower()));
    }

    public async Task<List<CatalogVehicleModel>> GetTransportVehicles()
    {
        var catalogVehicles = await _vehicleCatalogService.GetAll();

        return catalogVehicles.FindAll(veh => _deliveryOptions.TransportVehicleMaxProducts.ContainsKey(veh.Model));
    }

    public int? GetVehicleTransportSize(string model)
    {
        if (!_deliveryOptions.TransportVehicleMaxProducts.ContainsKey(model))
        {
            return null;
        }

        return _deliveryOptions.TransportVehicleMaxProducts[model];
    }
}