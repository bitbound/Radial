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
        event EventHandler<string> Disconnected;

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
        private readonly IHttpContextAccessor _httpContext;
        private Action _queuedInput;
        private RadialUser _user;
        public ClientConnection(IClientManager clientManager, 
            IHttpContextAccessor httpContextAccessor,
            IDataService dataService)
        {
            _clientManager = clientManager;
            _httpContext = httpContextAccessor;
            _dataService = dataService;
        }

        public event EventHandler<string> Disconnected;

        public event EventHandler<IMessageBase> MessageReceived;
        public RadialUser User
        {
            get
            {
                if (_httpContext?.HttpContext?.User?.Identity?.IsAuthenticated != true)
                {
                    return null;
                }
                
                return _user ??= _dataService.LoadUser(_httpContext.HttpContext.User.Identity.Name).GetAwaiter().GetResult();
            }
        }

        public Task Connect()
        {
            if (_httpContext?.HttpContext?.User?.Identity?.IsAuthenticated == true)
            {
                _dataService.InitializePlayer(User);
                _clientManager.AddClient(_httpContext.HttpContext.Connection.Id, this);
            }
            return Task.CompletedTask;
        }

        public void Disconnect(string reason)
        {
            Disconnected?.Invoke(this, reason);
        }

        public void Dispose()
        {
            _clientManager.RemoveClient(_httpContext?.HttpContext?.Connection?.Id);

            GC.SuppressFinalize(this);
        }

        public Action ExchangeInput(Action inputAction)
        {
            return Interlocked.Exchange(ref _queuedInput, inputAction);
        }

        public void InvokeMessageReceived(IMessageBase message)
        {
            if (message.MessageType == MessageType.CharacterInfoUpdated)
            {
                _dataService.ReloadEntity(User);
            }
            _ = Task.Run(() => MessageReceived?.Invoke(this, message));
        }
    }
}
