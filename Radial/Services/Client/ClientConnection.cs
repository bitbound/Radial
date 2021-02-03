using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
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
        event EventHandler<IMessageBase> MessageReceived;
        event EventHandler<string> Disconnected;
        RadialUser User { get; }
        void InvokeMessageReceived(IMessageBase message);
        Task Connect();
        void Disconnect(string reason);
    }

    public class ClientConnection : IClientConnection
    {
        private readonly IClientManager _clientManager;
        private readonly IHttpContextAccessor _httpContext;
        private readonly AuthenticationStateProvider _authStateProvider;
        private UserManager<RadialUser> _userManager;

        public ClientConnection(IClientManager clientManager, 
            IHttpContextAccessor httpContextAccessor,
            AuthenticationStateProvider authStateProvider,
            UserManager<RadialUser> userManager)
        {
            _clientManager = clientManager;
            _httpContext = httpContextAccessor;
            _authStateProvider = authStateProvider;
            _userManager = userManager;
        }

        public event EventHandler<IMessageBase> MessageReceived;
        
        public event EventHandler<string> Disconnected;

        public RadialUser User { get; private set; }

        public async Task Connect()
        {
            if (_httpContext?.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                User = await _userManager.GetUserAsync(_httpContext.HttpContext.User);
                _clientManager.AddClient(_httpContext.HttpContext.Connection.Id, this);
            }
        }

        public void Dispose()
        {
            _clientManager.RemoveClient(_httpContext?.HttpContext?.Connection?.Id);

            GC.SuppressFinalize(this);
        }

        public void InvokeMessageReceived(IMessageBase message)
        {
            MessageReceived?.Invoke(this, message);
        }

        public void Disconnect(string reason)
        {
            Disconnected?.Invoke(this, reason);
        }
    }
}
