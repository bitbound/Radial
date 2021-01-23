using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IGameHub
    {

    }

    [Authorize]
    public class GameHub : Hub, IGameHub
    {
        public static ConcurrentDictionary<string, IClientProxy> ClientConnections { get; } =
            new ConcurrentDictionary<string, IClientProxy>();
        public const string HubPath = "/game-hub";

        public override Task OnConnectedAsync()
        {
            ClientConnections.AddOrUpdate(Context.User.Identity.Name, Clients.Caller, (k,v) => Clients.Caller);
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            ClientConnections.Remove(Context.User.Identity.Name, out _);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
