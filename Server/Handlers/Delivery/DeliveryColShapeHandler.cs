using System.Threading.Tasks;
using AltV.Net;
using AltV.Net.Async;
using AltV.Net.Data;
using AltV.Net.Elements.Entities;
using Microsoft.Extensions.Options;
using Server.Core.Abstractions.ScriptStrategy;
using Server.Core.Configuration;
using Server.Core.Entities;
using Server.Core.Extensions;
using Server.DataAccessLayer.Services;
using Server.Modules.Delivery;

namespace Server.Handlers.Delivery
{
    public class DeliveryColShapeHandler
        : ISingletonScript
    {
        private readonly IColShape _deliveryPointColShape;

        public DeliveryColShapeHandler(
            IOptions<WorldLocationOptions> worldLocationOptions,
            DeliveryService deliveryService,
            DeliveryModule deliveryModule)
        {
            _deliveryPointColShape = Alt.CreateColShapeSphere(new Position(worldLocationOptions.Value.HarbourSelectionPositionX,
                                                                           worldLocationOptions.Value.HarbourSelectionPositionY,
                                                                           worldLocationOptions.Value.HarbourSelectionPositionZ),
                                                              4f);
  
            AltAsync.OnColShape += ((IColShape colShape, IEntity targetEntity, bool state) => OnColShape(colShape, targetEntity, state));
        }

        private async Task OnColShape(IColShape colShape, IEntity targetEntity, bool state)
        {
            if (colShape != _deliveryPointColShape)
            {
                return;
            }

            if (targetEntity is not ServerPlayer player)
            {
                return;
            }

            if (!player.Exists)
            {
                return;
            }

            if (state)
            {
                player.SendSubtitle("Du kannst mit ~y~/collect~w~ die Waren aufladen.", 3000);
            }
            else
            {
                player.ClearSubtitle();
            }
        }
    }
}