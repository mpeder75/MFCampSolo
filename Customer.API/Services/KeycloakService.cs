using Customer.API.Models;
using Dapr.Client;
using MFCampShared.Messages.Customer;
namespace Customer.API.Services
{
    public interface IKeycloakService
    {
        Task<CustomerResultMessage> CreateUserAsync(CustomerMessage customer);
    }

    public class KeycloakService : IKeycloakService
    {
        private readonly DaprClient _daprClient;
        private readonly ILogger<KeycloakService> _logger;

        public KeycloakService(DaprClient daprClient, ILogger<KeycloakService> logger)
        {
            _daprClient = daprClient;
            _logger = logger;
        }

        public async Task<CustomerResultMessage> CreateUserAsync(CustomerMessage customer)
        {
            _logger.LogInformation("Starting CreateUserAsync for Customer ID: {CustomerId}, Name: {CustomerName}", customer.ID, customer.Name);

            try
            {
                _logger.LogDebug("Invoking Keycloak API to create user with ID: {CustomerId}", customer.ID);

                var response = await _daprClient.InvokeMethodAsync<CustomerMessage, HttpResponseMessage>(
                    HttpMethod.Post,
                    "keycloak",
                    "admin/realms/MFCamp/users",
                    customer);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Successfully created user in Keycloak for Customer ID: {CustomerId}", customer.ID);

                    return new CustomerResultMessage
                    {
                        ID = customer.ID,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Status = "Success",
                        WorkflowId = customer.WorkflowId
                    };
                }
                else
                {
                    _logger.LogWarning("Failed to create user in Keycloak for Customer ID: {CustomerId}. Status Code: {StatusCode}", customer.ID, response.StatusCode);

                    return new CustomerResultMessage
                    {
                        ID = customer.ID,
                        Name = customer.Name,
                        Email = customer.Email,
                        Phone = customer.Phone,
                        Status = "Failed",
                        Error = $"Error: {response.StatusCode}",
                        WorkflowId = customer.WorkflowId
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating user in Keycloak for Customer ID: {CustomerId}", customer.ID);

                return new CustomerResultMessage
                {
                    ID = customer.ID,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Status = "Failed",
                    Error = ex.Message,
                    WorkflowId = customer.WorkflowId
                };
            }
            finally
            {
                _logger.LogInformation("Finished CreateUserAsync for Customer ID: {CustomerId}", customer.ID);
            }
        }
    }
}

