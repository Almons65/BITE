using Microsoft.AspNetCore.SignalR;

namespace BITE.Api.Hubs;

public class OrderHub : Hub
{
    public async Task SendStatusUpdate(string orderId, string newStatus)
    {
        await Clients.All.SendAsync("ReceiveStatusUpdate", orderId, newStatus);
    }
}