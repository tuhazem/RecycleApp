using RecyclingApp.Application.Features.Orders.DTOs;
using System;
using System.Threading.Tasks;

namespace RecyclingApp.API.Hubs;

public interface IPickUpClient
{
    Task ReceiveNewPickupOrder(PickupOrderResponseDto order);
    Task OrderStatusUpdated(Guid orderId, string newStatus);
}
