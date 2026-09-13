using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace RecyclingApp.API.Hubs;

public class PickUpHub : Hub<IPickUpClient>
{
    public const string AdminGroup = "Admins";

    public async Task JoinAdminGroup()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, AdminGroup);
    }

    public async Task LeaveAdminGroup()
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminGroup);
        await base.OnDisconnectedAsync(exception);
    }
}
