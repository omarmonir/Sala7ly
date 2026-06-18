using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sala7ly.API.Hubs
{
    [Authorize]
    public class BiddingHub : Hub
    {
        public async Task JoinRequest(string requestId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"request-{requestId}");
        }

        public async Task LeaveRequest(string requestId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"request-{requestId}");
        }
    }
}
