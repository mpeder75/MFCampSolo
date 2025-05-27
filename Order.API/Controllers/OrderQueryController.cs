using Microsoft.AspNetCore.Mvc;
using Order.API.Models;
using Order.Application.Commands.CommandDto;
using Order.Application.Queries;
using Order.Application.Queries.Common;
using Order.Application.Queries.QueryDto;
using Order.Application.ReadModels.ReadDto;
using Order.Application.Services;

namespace Order.API.Controllers;

[ApiController]
[Route("orders")]
public class OrderQueryController : ControllerBase
{
    private readonly ILogger<OrderQueryController> _logger;
    private readonly IQueryDispatcher _queryDispatcher;

    public OrderQueryController(ILogger<OrderQueryController> logger, IQueryDispatcher queryDispatcher)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _queryDispatcher = queryDispatcher ?? throw new ArgumentNullException(nameof(queryDispatcher));
    }
    /*
    [HttpGet]
    [Route("")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    */
    [HttpGet]
    [Route("")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetOrders(
        [FromQuery] int pageNumber = 1, 
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting orders page {PageNumber} with size {PageSize}", 
                pageNumber, pageSize);
            
            var query = new GetOrdersQuery(pageNumber, pageSize);
            var result = await _queryDispatcher.DispatchAsync<GetOrdersQuery, PagedResult<OrderSummaryDto>>(query);
            
            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving orders");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
    /*
    [HttpGet("customer/{customerId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    */
    [HttpGet]
    [Route("customer/{customerId:guid}")]
    public async Task<ActionResult<PagedResult<OrderHistoryDto>>> GetCustomerOrders(Guid customerId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting orders for customer {CustomerId}, page {PageNumber}", 
                customerId, pageNumber);
        
            var query = new GetCustomerOrdersQuery 
            { 
                CustomerId = customerId, 
                PageNumber = pageNumber, 
                PageSize = pageSize 
            };

            var result = await _queryDispatcher.DispatchAsync<GetCustomerOrdersQuery, PagedResult<OrderHistoryDto>>(query);
        
            if (result.Items.Count == 0)
            {
                return NotFound(new { message = $"No orders found for customer {customerId}" });
            }
        
            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving customer orders");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
    /*
    [HttpGet("status/{status}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    */
    [HttpGet]
    [Route("status/{status}")]
    public async Task<ActionResult<PagedResult<OrderSummaryDto>>> GetOrdersByStatus(
        string status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        try
        {
            _logger.LogInformation("Getting orders with status {Status}, page {PageNumber}", 
                status, pageNumber);
        
            var query = new GetOrdersByStatusQuery 
            { 
                Status = status,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        
            var result = await _queryDispatcher.DispatchAsync<GetOrdersByStatusQuery, PagedResult<OrderSummaryDto>>(query);
        
            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving orders by status");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
    /*
    [HttpGet("{orderId:guid}/details")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(Guid orderId)
    */
    [HttpGet]
    [Route("{orderId:guid}/details")]
    public async Task<ActionResult<OrderDetailsDto>> GetOrderDetails(Guid orderId)
    {
        try
        {
            _logger.LogInformation("Getting details for order {OrderId}", orderId);

            var query = new GetOrderDetailsQuery(orderId);
            var result = await _queryDispatcher.DispatchAsync<GetOrderDetailsQuery, OrderDetailsDto>(query);

            if (result == null)
            {
                return NotFound(new { message = $"Order {orderId} not found" });
            }

            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving order details");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
    /*
    [HttpGet("{orderId:guid}")]
    */
    
    [HttpGet]
    [Route("{orderId:guid}")]
    public async Task<ActionResult<OrderSummaryDto>> GetOrderSummary(Guid orderId)
    {
        try
        {
            var query = new GetOrderSummaryQuery(orderId);
            var result = await _queryDispatcher.DispatchAsync<GetOrderSummaryQuery, OrderSummaryDto>(query);
        
            if (result == null)
            {
                return NotFound(new { message = $"Order {orderId} not found" });
            }
        
            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error retrieving order summary");
            return StatusCode(500, new { error = "An error occurred while processing your request." });
        }
    }
}