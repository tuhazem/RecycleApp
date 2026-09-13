using MediatR;
using Microsoft.EntityFrameworkCore;
using RecyclingApp.Application.Common.Interfaces;
using RecyclingApp.Application.Features.Orders.DTOs;
using RecyclingApp.Domain.Entities;
using RecyclingApp.Domain.Enums;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RecyclingApp.Application.Features.Orders.Commands.CreatePickupOrder;

public record CreatePickupOrderCommand(string CustomerId, CreatePickupOrderDto Dto) : IRequest<CreatePickupOrderResult>;

public record CreatePickupOrderResult(
    bool Succeeded,
    string? Message,
    PickupOrderResponseDto? Order = null);

public class CreatePickupOrderCommandHandler : IRequestHandler<CreatePickupOrderCommand, CreatePickupOrderResult>
{
    private readonly IApplicationDbContext _context;

    public CreatePickupOrderCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreatePickupOrderResult> Handle(CreatePickupOrderCommand request, CancellationToken cancellationToken)
    {
        var customerId = request.CustomerId;
        var dto = request.Dto;

        if (string.IsNullOrWhiteSpace(customerId))
        {
            return new CreatePickupOrderResult(false, "CustomerId is required.");
        }

        // 1. Validate that the Customer exists in the database
        var customer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == customerId, cancellationToken);

        if (customer == null)
        {
            return new CreatePickupOrderResult(false, $"Customer with ID '{customerId}' was not found in the system.");
        }

        if (dto.PickupTime <= DateTime.UtcNow)
        {
            return new CreatePickupOrderResult(false, "Pickup time must be scheduled in the future.");
        }

        if (dto.Items == null || !dto.Items.Any())
        {
            return new CreatePickupOrderResult(false, "At least one item is required to create a pickup order.");
        }

        // 2. Resolve Customer Name and Phone (auto-fallback to profile if omitted)
        var customerName = !string.IsNullOrWhiteSpace(dto.CustomerName)
            ? dto.CustomerName.Trim()
            : (!string.IsNullOrWhiteSpace(customer.FullName) ? customer.FullName : customer.UserName ?? "Customer");

        var customerPhone = !string.IsNullOrWhiteSpace(dto.Phone)
            ? dto.Phone.Trim()
            : (!string.IsNullOrWhiteSpace(customer.PhoneNumber) ? customer.PhoneNumber : "N/A");

        // 3. Validate and load Products from the database
        var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
        var existingProducts = await _context.Products
            .Where(p => productIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, cancellationToken);

        // Check for missing or inactive products
        foreach (var item in dto.Items)
        {
            if (!existingProducts.TryGetValue(item.ProductId, out var product))
            {
                return new CreatePickupOrderResult(false, $"Product with ID '{item.ProductId}' was not found.");
            }

            if (!product.IsActive)
            {
                return new CreatePickupOrderResult(false, $"Product '{product.Name}' (ID: {item.ProductId}) is currently inactive and cannot be ordered.");
            }

            if (item.Quantity <= 0)
            {
                return new CreatePickupOrderResult(false, $"Quantity for product '{product.Name}' must be greater than zero.");
            }
        }

        // 4. Create Order Aggregate with server-authoritative product prices
        var order = new Order(
            customerId,
            customerName,
            customerPhone,
            dto.Notes,
            dto.PickupTime,
            OrderType.Pickup);

        foreach (var item in dto.Items)
        {
            var product = existingProducts[item.ProductId];
            
            // Server always pulls official price and name from database
            var unitPrice = product.Price.Amount;
            var productName = product.Name;

            order.AddItem(product.Id, productName, unitPrice, item.Quantity);
        }

        _context.Orders.Add(order);
        await _context.SaveChangesAsync(cancellationToken);

        var responseDto = new PickupOrderResponseDto(
            order.Id,
            order.CustomerId,
            order.CustomerName,
            order.Phone,
            order.Notes,
            order.PickupTime,
            order.OrderType.ToString(),
            order.Status.ToString(),
            order.TotalAmount,
            order.CreatedAtUtc,
            order.Items.Select(i => new OrderItemResponseDto(
                i.Id,
                i.ProductId,
                i.ProductName,
                i.UnitPrice,
                i.Quantity,
                i.SubTotal)).ToList());

        return new CreatePickupOrderResult(true, "Pickup order created successfully.", responseDto);
    }
}
