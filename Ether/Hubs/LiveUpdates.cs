using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Ether.Hubs
{
    [Authorize]
    public class LiveUpdatesHub : Hub
    {
    }
}
