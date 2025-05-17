using Dapr.Client;
using MFCampShared.Messages.Customer;

namespace Customer.API.Services
{

    public interface ICustomerService
    {
        Task<CustomerResultMessage> CreateCustomerAsync(CustomerMessage customer);
        Task<CustomerResultMessage> GetCustomerAsync(string id);
        Task<CustomerResultMessage> UpdateCustomerAsync(CustomerMessage customer);
        Task<CustomerResultMessage> DeleteCustomerAsync(CustomerMessage customer);
    }

    public class CustomerService : ICustomerService
    {
        private readonly DaprClient _dapr;
        private readonly ILogger<CustomerService> _logger;
        private const string STORE_NAME = "customerstatestore";
        private const string PUBSUB_NAME = "pubsub";

        public CustomerService(DaprClient daprClient, ILogger<CustomerService> logger)
        {
            _dapr = daprClient;
            _logger = logger;
        }

        public async Task<CustomerResultMessage> CreateCustomerAsync(CustomerMessage customer)
        {
            if (customer == null)
            {
                _logger.LogError("CreateCustomerAsync called with null customer.");
                throw new ArgumentNullException(nameof(customer), "Customer cannot be null.");
            }

            var newCustomer = Models.Customer.Create(Guid.NewGuid(), customer.Name, customer.Email, customer.Phone);

            try
            {
                await _dapr.SaveStateAsync(STORE_NAME, newCustomer.ID.ToString(), newCustomer);
                _logger.LogInformation("Customer state saved with ID: {CustomerId}", newCustomer.ID);

                await _dapr.PublishEventAsync(PUBSUB_NAME, "customercreated", newCustomer);
                _logger.LogInformation("Customer created event published for ID: {CustomerId}", newCustomer.ID);

                return new CustomerResultMessage
                {
                    ID = newCustomer.ID.ToString(),
                    Name = newCustomer.Name,
                    Email = newCustomer.Email,
                    Phone = newCustomer.Phone,
                    Status = "Created",
                    Error = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating customer with ID: {CustomerId}", newCustomer.ID);
                throw;
            }
        }

        public async Task<CustomerResultMessage> GetCustomerAsync(string id)
        {
            if (string.IsNullOrWhiteSpace(id) || !Guid.TryParse(id, out var customerId))
            {
                _logger.LogError("GetCustomerAsync called with an invalid ID.");
                throw new ArgumentException("Customer ID cannot be null, empty, or invalid.", nameof(id));
            }

            try
            {
                var customer = await _dapr.GetStateAsync<Models.Customer>(STORE_NAME, customerId.ToString());
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found.", customerId);
                    return new CustomerResultMessage
                    {
                        ID = id,
                        Status = "NotFound",
                        Error = $"Customer with ID {id} not found."
                    };
                }

                _logger.LogInformation("Customer with ID: {CustomerId} retrieved successfully.", customerId);
                return new CustomerResultMessage
                {
                    ID = customer.ID.ToString(),
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Status = "Retrieved",
                    Error = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving customer with ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerResultMessage> UpdateCustomerAsync(CustomerMessage customer)
        {
            if (customer == null || string.IsNullOrWhiteSpace(customer.ID) || !Guid.TryParse(customer.ID, out var customerId))
            {
                _logger.LogError("UpdateCustomerAsync called with null or invalid customer.");
                throw new ArgumentException("Customer or Customer ID cannot be null, empty, or invalid.", nameof(customer));
            }

            try
            {
                var existingCustomer = await _dapr.GetStateAsync<Models.Customer>(STORE_NAME, customerId.ToString());
                if (existingCustomer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found for update.", customerId);
                    return new CustomerResultMessage
                    {
                        ID = customer.ID,
                        Status = "NotFound",
                        Error = $"Customer with ID {customer.ID} not found."
                    };
                }

                existingCustomer.Update(customer.Name, customer.Email, customer.Phone);

                await _dapr.SaveStateAsync(STORE_NAME, existingCustomer.ID.ToString(), existingCustomer);
                _logger.LogInformation("Customer state updated with ID: {CustomerId}", existingCustomer.ID);

                await _dapr.PublishEventAsync(PUBSUB_NAME, "customerupdated", existingCustomer);
                _logger.LogInformation("Customer updated event published for ID: {CustomerId}", existingCustomer.ID);

                return new CustomerResultMessage
                {
                    ID = existingCustomer.ID.ToString(),
                    Name = existingCustomer.Name,
                    Email = existingCustomer.Email,
                    Phone = existingCustomer.Phone,
                    Status = "Updated",
                    Error = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID: {CustomerId}", customerId);
                throw;
            }
        }

        public async Task<CustomerResultMessage> DeleteCustomerAsync(CustomerMessage customerMessage)
        {
            if (customerMessage == null || string.IsNullOrWhiteSpace(customerMessage.ID) || !Guid.TryParse(customerMessage.ID, out var customerId))
            {
                _logger.LogError("DeleteCustomerAsync called with null or invalid CustomerMessage.");
                throw new ArgumentException("CustomerMessage or Customer ID cannot be null, empty, or invalid.", nameof(customerMessage));
            }

            try
            {
                var customer = await _dapr.GetStateAsync<Models.Customer>(STORE_NAME, customerId.ToString());
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID: {CustomerId} not found for deletion.", customerId);
                    return new CustomerResultMessage
                    {
                        ID = customerMessage.ID,
                        Name = customerMessage.Name,
                        Email = customerMessage.Email,
                        Phone = customerMessage.Phone,
                        Status = "NotFound",
                        Error = $"Customer with ID {customerId} not found."
                    };
                }

                await _dapr.DeleteStateAsync(STORE_NAME, customerId.ToString());
                _logger.LogInformation("Customer state deleted with ID: {CustomerId}", customerId);

                await _dapr.PublishEventAsync(PUBSUB_NAME, "customerdeleted", customer);
                _logger.LogInformation("Customer deleted event published for ID: {CustomerId}", customerId);

                return new CustomerResultMessage
                {
                    ID = customerMessage.ID,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Status = "Deleted",
                    Error = null
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting customer with ID: {CustomerId}", customerId);
                return new CustomerResultMessage
                {
                    ID = customerMessage.ID,
                    Name = customerMessage.Name,
                    Email = customerMessage.Email,
                    Phone = customerMessage.Phone,
                    Status = "Error",
                    Error = ex.Message
                };
            }
        }
    }
}
