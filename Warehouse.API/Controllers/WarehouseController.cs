using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using Warehouse.API.Models;
using Warehouse.API.Services;
using MFCampShared.Messages.Warehouse;
using Dapr;


namespace Warehouse.API.Controllers
{
    public class WarehouseController : ControllerBase
    {
        private readonly DaprClient _daprClient;
        private readonly IWarehouseStateService _warehouseStateService;
        private readonly ILogger<WarehouseController> _logger;
        private const string PUBSUB_NAME = "pubsub";

        public WarehouseController(DaprClient daprClient, IWarehouseStateService warehouseStateService, ILogger<WarehouseController> logger)
        {
            _daprClient = daprClient;
            _warehouseStateService = warehouseStateService;
            _logger = logger;
        }

        [Topic("pubsub", "shipping")]
        [HttpPost("ware-created")]
        public async Task<ActionResult<Ware>> CreateWareAsync(WareMessage ware)
        {
            _logger.LogInformation("Creating new ware with ID: {WareId}", ware.ID);

            var result = await _warehouseStateService.CreateWareAsync(ware);
            await _daprClient.PublishEventAsync(PUBSUB_NAME, "workflow-warehouse", result);

            return Ok(result);
        }
    }
}
