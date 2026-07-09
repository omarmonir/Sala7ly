using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Sala7ly.API.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        
    }
}