using Dapr.Client;
using Microsoft.Extensions.Logging;
using Warehouse.API.Models;
using MFCampShared.Messages.Warehouse;

namespace Warehouse.API.Services
{
    public interface IWarehouseStateService
    {
        Task<WareResultMessage> CreateWareAsync(WareMessage wareMessage);

    }

    public class WarehouseStateService : IWarehouseStateService
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<WarehouseStateService> _logger;
        private const string STORE_NAME = "warehousestatestore";
        private const string PUBSUB_NAME = "pubsub";
        private const string TOPIC_NAME = "warehouse";

        public WarehouseStateService(DaprClient daprClient, ILogger<WarehouseStateService> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        async Task<WareResultMessage> IWarehouseStateService.CreateWareAsync(WareMessage wareMessage)
        {
            var stateKey = $"ware_{wareMessage.ID}";
            Ware saveWare = new Ware
            {
                ID = Guid.NewGuid(),
                Name = wareMessage.Name,
                Description = wareMessage.Description
            };

            var result = new WareResultMessage
            {
                WorkflowId = wareMessage.WorkflowId,
                Name = wareMessage.Name,
                Description = wareMessage.Description,
            };

            try
            {
                await _daprClient.SaveStateAsync<Ware>(STORE_NAME, stateKey, saveWare);
                _logger.LogInformation("Ware created sucessfully with WareId: {WareId}", saveWare.ID);
                result.Status = "Successful";

                await _daprClient.PublishEventAsync(PUBSUB_NAME, TOPIC_NAME, result);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create ware with ID: {WareId}", wareMessage.ID);
                throw new InvalidOperationException($"Error creating ware with ID: {wareMessage.ID}", ex);
            }
        }
    }
}
