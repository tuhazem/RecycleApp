using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RecyclingApp.Application.Features.Categories.Commands.CreateCategory;
using RecyclingApp.Application.Features.Categories.Commands.DeleteCategory;
using RecyclingApp.Application.Features.Categories.Commands.UpdateCategory;
using RecyclingApp.Application.Features.Categories.Queries.GetAllCategories;
using RecyclingApp.Application.Features.Categories.Queries.GetCategoryById;

namespace RecyclingApp.API.Controllers;

/// <summary>
/// Controller for Category management endpoints.
/// </summary>
[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly IMediator _mediator;

    public CategoriesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreateCategoryCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Succeeded)
        {
            return BadRequest(result);
        }
        return CreatedAtAction(nameof(GetById), new { id = result.CategoryId }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryCommand command)
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
        var result = await _mediator.Send(new DeleteCategoryCommand(id));
        if (!result.Succeeded)
        {
            return NotFound(result);
        }
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetCategoryByIdQuery(id));
        if (result is null)
        {
            return NotFound(new { Message = "Category not found." });
        }
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool onlyActive = true)
    {
        var result = await _mediator.Send(new GetAllCategoriesQuery(onlyActive));
        return Ok(result);
    }
}
