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

namespace Radial.Services.Client
{
    public interface IClientConnection : IDisposable
    {
        event EventHandler<IMessageBase> MessageReceived;
        event EventHandler<string> Disconnected;

        PlayerInfo PlayerInfo { get; }

        void InvokeMessageReceived(IMessageBase message);
        void Connect();
        void Disconnect(string reason);
    }

    public class ClientConnection : IClientConnection
    {
        public PlayerInfo PlayerInfo { get; } = new PlayerInfo();

        private readonly IClientManager _clientManager;
        private readonly IHttpContextAccessor _httpContext;

        public ClientConnection(IClientManager clientManager, 
            IHttpContextAccessor httpContextAccessor)
        {
            _clientManager = clientManager;
            _httpContext = httpContextAccessor;

            PlayerInfo.Username = _httpContext?.HttpContext?.User?.Identity?.Name;
        }

        public event EventHandler<IMessageBase> MessageReceived;
        
        public event EventHandler<string> Disconnected;

        public void Connect()
        {
            if (_httpContext?.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
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
