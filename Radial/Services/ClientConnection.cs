using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Radial.Models.Dtos;
using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IClientConnection : IDisposable
    {
        event EventHandler<ChatMessageDto> ChatReceived;
        event EventHandler<string> Disconnected;

        string Username { get; }

        void InvokeDtoReceived(IBaseDto dto);
        void Connect();
        void Disconnect(string reason);
    }

    public class ClientConnection : IClientConnection
    {
        public string Username { get; }

        private readonly IClientManager _clientManager;
        private readonly IHttpContextAccessor _httpContext;

        public ClientConnection(IClientManager clientManager, 
            IHttpContextAccessor httpContextAccessor)
        {
            _clientManager = clientManager;
            _httpContext = httpContextAccessor;

            Username = _httpContext?.HttpContext?.User?.Identity?.Name;
        }

        public event EventHandler<ChatMessageDto> ChatReceived;
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

        public void InvokeDtoReceived(IBaseDto dto)
        {
            switch (dto.DtoType)
            {
                case DtoType.ChatMessage:
                    ChatReceived?.Invoke(this, dto as ChatMessageDto);
                    break;
                default:
                    break;
            }
        }

        public void Disconnect(string reason)
        {
            Disconnected?.Invoke(this, reason);
        }
    }
}
