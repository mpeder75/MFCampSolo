using MFCampShared.Messages.Customer;
using Customer.API.Services;
using Dapr.Client;
using Customer.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace Customer.API.Controllers
{
    public class KeycloakController : ControllerBase
    {
        private readonly IKeycloakService _keycloakService;

        public KeycloakController(IKeycloakService keycloakService)
        {
            _keycloakService = keycloakService;
        }


        [HttpPost("create-user")]
        public async Task<ActionResult<CustomerResultMessage>> CreateUserAsync(CustomerMessage customerMessage)
        {
            if (customerMessage == null || string.IsNullOrWhiteSpace(customerMessage.Name) || string.IsNullOrWhiteSpace(customerMessage.Email))
            {
                return BadRequest(new CustomerResultMessage
                {
                    Status = "Error",
                    Error = "Invalid customer data. Name and Email are required."
                });
            }

            try
            {
                var result = await _keycloakService.CreateUserAsync(customerMessage);
                if (result != null && result.Status == "Success")
                {
                    return Ok(result);
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new CustomerResultMessage
                {
                    Status = "Error",
                    Error = "Failed to create user."
                });
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new CustomerResultMessage
                {
                    Status = "Error",
                    Error = $"Error creating user: {ex.Message}"
                });
            }
        }

    }
}
