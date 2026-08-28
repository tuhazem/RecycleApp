using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RecyclingApp.Application.Common.Models;
using RecyclingApp.Application.Features.Products.Commands.AdjustStock;
using RecyclingApp.Application.Features.Products.Commands.CreateProduct;
using RecyclingApp.Application.Features.Products.Commands.DeleteProduct;
using RecyclingApp.Application.Features.Products.Commands.UpdateProduct;
using RecyclingApp.Application.Features.Products.DTOs;
using RecyclingApp.Application.Features.Products.Queries.GetPaginatedProducts;
using RecyclingApp.Application.Features.Products.Queries.GetProductById;
using RecyclingApp.Application.Features.Products.Queries.GetProductsByCategory;
using System;
using System.Threading.Tasks;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for Product management endpoints.
/// </summary>
[ApiController]
[Route("api/products")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.ProductId }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductCommand command)
    {
        if (id != command.Id)
        {
            return BadRequest(new { Message = "Route ID does not match command ID." });
        }
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var result = await _mediator.Send(new DeleteProductCommand(id));
        if (!result.Succeeded)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpPost("{id:guid}/adjust-stock")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AdjustStock(Guid id, [FromBody] AdjustStockRequest request)
    {
        var result = await _mediator.Send(new AdjustProductStockCommand(id, request.Delta));
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetProductByIdQuery(id));
        if (result is null)
        {
            return NotFound(new { Message = "Product not found." });
        }
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a paginated list of products for a specific category using EF Core AsNoTracking().
    /// </summary>
    /// <param name="categoryId">Unique identifier of the Category</param>
    /// <param name="pageIndex">Current page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    [HttpGet("category/{categoryId:guid}")]
    [ProducesResponseType(typeof(PagedResult<ProductDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result<PagedResult<ProductDto>>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCategory(
        Guid categoryId,
        [FromQuery] int pageIndex = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetProductsByCategoryQuery(categoryId, pageIndex, pageSize);
        var result = await _mediator.Send(query);
        if (!result.Succeeded)
        {
            return NotFound(result);
        }
        return Ok(result.Data);
    }

    /// <summary>
    /// Retrieves a paginated list of products with optional search, sorting, and category filters.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetPaginated(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] bool sortDescending = false,
        [FromQuery] Guid? categoryId = null,
        [FromQuery] decimal? minPrice = null,
        [FromQuery] decimal? maxPrice = null,
        [FromQuery] bool? isActive = null)
    {
        var query = new GetPaginatedProductsQuery(
            pageNumber,
            pageSize,
            searchTerm,
            sortBy,
            sortDescending,
            categoryId,
            minPrice,
            maxPrice,
            isActive);

        var result = await _mediator.Send(query);
        return Ok(result);
    }
}

public record AdjustStockRequest(int Delta);
