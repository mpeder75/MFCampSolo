using Microsoft.AspNetCore.Mvc;
using Customer.API.Services;
using MFCampShared.Messages.Customer;
using System.Numerics;

namespace Customer.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CustomerController : ControllerBase
    {
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerController> _logger;

        public CustomerController(ICustomerService customerService, ILogger<CustomerController> logger)
        {
            _customerService = customerService;
            _logger = logger;
        }

        [HttpPost("CreateCustomer")]
        public async Task<ActionResult<CustomerResultMessage>> CreateCustomerAsync(CustomerMessage customerMessage)
        {
            if (customerMessage == null)
            {
                _logger.LogWarning("CreateCustomerAsync called with null CustomerMessage.");
                return BadRequest("CustomerMessage cannot be null.");
            }

            try
            {
                _logger.LogInformation("Creating a new customer with Name: {Name}, Email: {Email}", customerMessage.Name, customerMessage.Email);
                var result = await _customerService.CreateCustomerAsync(customerMessage);

                if (result == null || result.Status != "Created")
                {
                    _logger.LogWarning("Failed to create customer. Status: {Status}, Error: {Error}", result?.Status, result?.Error);
                    return BadRequest(result?.Error ?? "Failed to create the customer.");
                }

                _logger.LogInformation("Customer created successfully with ID: {ID}", result.ID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a customer.");
                return StatusCode(500, "An error occurred while creating the customer.");
            }
        }

        [HttpGet("GetCustomer/{id}")]
        public async Task<ActionResult<CustomerResultMessage>> GetCustomerAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("GetCustomerAsync called with an empty GUID.");
                return BadRequest("Customer ID cannot be empty.");
            }

            try
            {
                _logger.LogInformation("Retrieving customer with ID: {ID}", id);
                var customer = await _customerService.GetCustomerAsync(id.ToString());
                if (customer == null)
                {
                    _logger.LogWarning("Customer with ID: {ID} not found.", id);
                    return NotFound($"Customer with ID {id} not found.");
                }

                var response = new CustomerResultMessage
                {
                    ID = customer.ID,
                    Name = customer.Name,
                    Email = customer.Email,
                    Phone = customer.Phone,
                    Status = "Retrieved"
                };

                _logger.LogInformation("Customer with ID: {ID} retrieved successfully.", id);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving customer with ID: {ID}", id);
                return StatusCode(500, "An error occurred while retrieving the customer.");
            }
        }

        [HttpPut("UpdateCustomer")]
        public async Task<ActionResult<CustomerResultMessage>> UpdateCustomerAsync(CustomerMessage customerMessage)
        {
            if (customerMessage == null)
            {
                _logger.LogWarning("UpdateCustomerAsync called with null CustomerMessage.");
                return BadRequest("CustomerMessage cannot be null.");
            }

            try
            {
                _logger.LogInformation("Updating customer with ID: {ID}", customerMessage.ID);
                var updatedCustomer = await _customerService.UpdateCustomerAsync(new CustomerMessage
                {
                    ID = customerMessage.ID,
                    Name = customerMessage.Name,
                    Email = customerMessage.Email,
                    Phone = customerMessage.Phone
                });

                if (updatedCustomer == null)
                {
                    _logger.LogWarning("Customer with ID: {ID} not found for update.", customerMessage.ID);
                    return NotFound($"Customer with ID {customerMessage.ID} not found.");
                }

                var response = new CustomerResultMessage
                {
                    ID = updatedCustomer.ID,
                    Name = updatedCustomer.Name,
                    Email = updatedCustomer.Email,
                    Phone = updatedCustomer.Phone,
                    Status = "Updated"
                };

                _logger.LogInformation("Customer with ID: {ID} updated successfully.", response.ID);
                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating customer with ID: {ID}", customerMessage.ID);
                return StatusCode(500, "An error occurred while updating the customer.");
            }
        }

        [HttpDelete("DeleteCustomer")]
        public async Task<ActionResult<CustomerResultMessage>> DeleteCustomerAsync(CustomerMessage customerMessage)
        {
            if (customerMessage == null)
            {
                _logger.LogWarning("DeleteCustomerAsync called with null CustomerMessage.");
                return BadRequest("CustomerMessage cannot be null.");
            }

            try
            {
                _logger.LogInformation("Deleting customer with ID: {ID}", customerMessage.ID);
                var result = await _customerService.DeleteCustomerAsync(customerMessage);
                if (result == null || result.Status != "Deleted")
                {
                    _logger.LogWarning("Failed to delete customer with ID: {ID}. Status: {Status}, Error: {Error}", customerMessage.ID, result?.Status, result?.Error);
                    return BadRequest(result?.Error ?? "Failed to delete the customer.");
                }

                _logger.LogInformation("Customer with ID: {ID} deleted successfully.", customerMessage.ID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting customer with ID: {ID}", customerMessage.ID);
                return StatusCode(500, "An error occurred while deleting the customer.");
            }
        }
    }
}