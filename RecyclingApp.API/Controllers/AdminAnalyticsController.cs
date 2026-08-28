using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Admin.Analytics.DTOs;
using RecyclingApp.Application.Features.Admin.Analytics.Queries.GetCustomerDirectory;
using RecyclingApp.Application.Features.Admin.Analytics.Queries.GetCustomerOrderHistory;
using RecyclingApp.Application.Features.Admin.Analytics.Queries.GetSalesTrend;
using RecyclingApp.Application.Features.Admin.Analytics.Queries.GetSummary;
using RecyclingApp.Application.Features.Admin.Analytics.Queries.GetTopCustomers;
using RecyclingApp.Application.Features.Admin.Analytics.Queries.GetTopProducts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for Admin Dashboard Analytics, Statistics, and Reporting.
/// Secured exclusively for administrators.
/// </summary>
[ApiController]
[Route("api/admin/analytics")]
[Authorize(Roles = "Admin")]
public class AdminAnalyticsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AdminAnalyticsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// 1. High-Level Executive Summary Metrics (Revenue, Orders breakdown, Customers, Stock status).
    /// </summary>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(Result<SummaryMetricsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSummary()
    {
        var result = await _mediator.Send(new GetAnalyticsSummaryQuery());
        return Ok(result);
    }

    /// <summary>
    /// 2. Top N Selling Products calculated by total quantity sold and revenue generated.
    /// </summary>
    /// <param name="count">Number of top products to retrieve (default: 5)</param>
    [HttpGet("top-products")]
    [ProducesResponseType(typeof(Result<List<TopProductDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopProducts([FromQuery] int count = 5)
    {
        var result = await _mediator.Send(new GetTopSellingProductsQuery(count));
        return Ok(result);
    }

    /// <summary>
    /// 3. Top N Big Spenders / High-Value Customers based on total money spent.
    /// </summary>
    /// <param name="count">Number of top customers to retrieve (default: 5)</param>
    [HttpGet("top-customers")]
    [ProducesResponseType(typeof(Result<List<TopCustomerDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTopCustomers([FromQuery] int count = 5)
    {
        var result = await _mediator.Send(new GetTopCustomersQuery(count));
        return Ok(result);
    }

    /// <summary>
    /// 4. Paginated Customer Directory with search and aggregated spend/order totals.
    /// </summary>
    /// <param name="pageIndex">Page number (1-based)</param>
    /// <param name="pageSize">Page size (default: 10)</param>
    /// <param name="searchTerm">Filter by customer name, email, username, or phone</param>
    [HttpGet("customers")]
    [ProducesResponseType(typeof(Result<PagedResult<CustomerDirectoryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCustomerDirectory(
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var result = await _mediator.Send(new GetCustomerDirectoryQuery(pageIndex, pageSize, searchTerm));
        return Ok(result);
    }

    /// <summary>
    /// 4b. Full purchase/order history for a specific customer.
    /// </summary>
    /// <param name="customerId">Unique identifier of the customer</param>
    [HttpGet("customers/{customerId}/orders")]
    [ProducesResponseType(typeof(Result<CustomerOrderHistoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetCustomerOrderHistory(string customerId)
    {
        var result = await _mediator.Send(new GetCustomerOrderHistoryQuery(customerId));
        if (!result.Succeeded)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    /// <summary>
    /// 5. Sales Performance Trends grouped dynamically by daily, weekly, or monthly intervals.
    /// </summary>
    /// <param name="period">Time interval: 'daily', 'weekly', or 'monthly' (default: monthly)</param>
    /// <param name="startDate">Optional start date filter</param>
    /// <param name="endDate">Optional end date filter</param>
    [HttpGet("sales-trend")]
    [ProducesResponseType(typeof(Result<SalesTrendDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetSalesTrend(
        [FromQuery] string period = "monthly",
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null)
    {
        var result = await _mediator.Send(new GetSalesTrendQuery(period, startDate, endDate));
        return Ok(result);
    }
}
