using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecyclingApp.Application.Features.Admin.Queries.GetDashboardStats;
using RecyclingApp.Application.Features.Products.Queries.GetLowStockProducts;
using RecyclingApp.Application.Features.Products.Queries.GetOutOfStockProducts;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for Admin Dashboard Analytics.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "Admin")]
public class AdminDashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminDashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("dashboard/stats")]
    public async Task<IActionResult> GetDashboardStats()
    {
        var result = await _mediator.Send(new GetAdminDashboardStatsQuery());
        return Ok(result);
    }

    [HttpGet("products/low-stock")]
    public async Task<IActionResult> GetLowStockProducts()
    {
        var result = await _mediator.Send(new GetLowStockProductsQuery());
        return Ok(result);
    }

    [HttpGet("products/out-of-stock")]
    public async Task<IActionResult> GetOutOfStockProducts()
    {
        var result = await _mediator.Send(new GetOutOfStockProductsQuery());
        return Ok(result);
    }
}
