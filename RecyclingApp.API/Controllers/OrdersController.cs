using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RecyclingApp.API.Hubs;
using RecyclingApp.Application.Features.Orders.Commands.CreatePickupOrder;
using RecyclingApp.Application.Features.Orders.DTOs;
using RecyclingApp.Application.Features.Orders.Queries.GetOrderById;
using System;
using System.Threading.Tasks;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for managing Customer and Pickup Orders.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IHubContext<PickUpHub, IPickUpClient> _hubContext;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IMediator mediator,
        IHubContext<PickUpHub, IPickUpClient> hubContext,
        ILogger<OrdersController> logger)
    {
        _mediator = mediator;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Creates a new pickup order and broadcasts real-time updates to Admin dashboard via SignalR.
    /// </summary>
    [HttpPost("pickup")]
    [ProducesResponseType(typeof(PickupOrderResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePickupOrder([FromBody] CreatePickupOrderDto dto)
    {
        var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var customerId = !string.IsNullOrWhiteSpace(dto.CustomerId) ? dto.CustomerId : currentUserId;

        if (string.IsNullOrWhiteSpace(customerId))
        {
            return BadRequest(new { message = "CustomerId is required (or the request must be authenticated with a valid JWT Bearer token)." });
        }

        var command = new CreatePickupOrderCommand(customerId, dto);
        var result = await _mediator.Send(command);

        if (!result.Succeeded || result.Order == null)
        {
            return BadRequest(new { message = result.Message });
        }

        // Broadcast real-time order notification to connected Admins
        try
        {
            await _hubContext.Clients
                .Group(PickUpHub.AdminGroup)
                .ReceiveNewPickupOrder(result.Order);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast new order {OrderId} to Admin group via SignalR.", result.Order.Id);
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Order.Id }, result.Order);
    }

    /// <summary>
    /// Retrieves a specific order by its unique ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PickupOrderResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var query = new GetOrderByIdQuery(id);
        var order = await _mediator.Send(query);

        if (order == null)
        {
            return NotFound(new { message = $"Order with ID '{id}' was not found." });
        }

        return Ok(order);
    }

    /// <summary>
    /// Updates the status of an order and broadcasts the update live over SignalR.
    /// Accessible by Admin only.
    /// Statuses: Pending = 1, Confirmed = 2, ReadyForPickup = 3, Completed = 4, Cancelled = 5
    /// </summary>
    [HttpPut("{id:guid}/status")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
    {
        var command = new RecyclingApp.Application.Features.Orders.Commands.UpdateOrderStatus.UpdateOrderStatusCommand(id, request.Status);
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
        {
            return BadRequest(new { message = result.Message });
        }

        // Broadcast status update live over SignalR to listening clients
        try
        {
            await _hubContext.Clients.All.OrderStatusUpdated(id, result.NewStatusName!);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast status update for order {OrderId} via SignalR.", id);
        }

        return Ok(new { message = result.Message, newStatus = result.NewStatusName });
    }
}

public record UpdateOrderStatusRequest(RecyclingApp.Domain.Enums.OrderStatus Status);
