using Radial.Models.Messaging;
using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Radial.Models;
using Microsoft.AspNetCore.Identity;
using Radial.Data.Entities;
using Microsoft.AspNetCore.Components.Authorization;

namespace Radial.Services.Client
{
    public interface IClientConnection : IDisposable
    {
        event EventHandler<string> Disconnected;

        Location Location { get; }

        event EventHandler<IMessageBase> MessageReceived;
        RadialUser User { get; }
        Task Connect();

        void Disconnect(string reason);

        Action ExchangeInput(Action inputAction);

        void InvokeMessageReceived(IMessageBase message);
    }

    public class ClientConnection : IClientConnection
    {
        private readonly IClientManager _clientManager;
        private readonly IDataService _dataService;
        private readonly AuthenticationStateProvider _authProvider;
        private Action _queuedInput;
        public ClientConnection(
            AuthenticationStateProvider authProvider,
            IClientManager clientManager, 
            IDataService dataService)
        {
            _authProvider = authProvider;
            _clientManager = clientManager;
            _dataService = dataService;
        }

        public event EventHandler<string> Disconnected;

        public event EventHandler<IMessageBase> MessageReceived;
        public RadialUser User
        {
            get
            {
                var authState = _authProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
                if (!authState.User.Identity.IsAuthenticated)
                {
                    return null;
                }
                
                return _dataService.LoadUser(authState.User.Identity.Name).GetAwaiter().GetResult();
            }
        }

        public Location Location
        {
            get
            {
                var authState = _authProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
                if (!authState.User.Identity.IsAuthenticated)
                {
                    return null;
                }
                return  _dataService.LoadLocation(User.Character.LocationId).GetAwaiter().GetResult();
            }
        }

        public async Task Connect()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            if (authState.User?.Identity?.IsAuthenticated == true)
            {
                await _clientManager.AddClient(this);
            }
        }

        public void Disconnect(string reason)
        {
            Disconnected?.Invoke(this, reason);
        }

        public void Dispose()
        {
            Disconnect("Session closed by server.");
            _clientManager.RemoveClient(this);

            GC.SuppressFinalize(this);
        }

        public Action ExchangeInput(Action inputAction)
        {
            return Interlocked.Exchange(ref _queuedInput, inputAction);
        }

        public void InvokeMessageReceived(IMessageBase message)
        {
            _ = Task.Run(() => MessageReceived?.Invoke(this, message));
        }
    }
}
