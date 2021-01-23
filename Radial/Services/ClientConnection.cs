using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.SignalR.Client;
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
        HubConnectionState State { get; }
        Task Connect();
    }

    public class ClientConnection : IClientConnection
    {
        private readonly SemaphoreSlim _connectionLock = new SemaphoreSlim(1, 1);
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly NavigationManager _navigationManager;
        private HubConnection _connection;
        private bool _isDisposed;

        public ClientConnection(NavigationManager navigationManager, IHttpContextAccessor httpContextAccessor)
        {
            _navigationManager = navigationManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public HubConnectionState State => _connection?.State ?? HubConnectionState.Disconnected;

        public async Task Connect()
        {
            if (!await _connectionLock.WaitAsync(0))
            {
                return;
            }

            try
            {
                if (_connection?.State == HubConnectionState.Connected)
                {
                    return;
                }

                if (_connection != null)
                {
                    try
                    {
                        await _connection.StopAsync();
                        await _connection.DisposeAsync();
                    }
                    catch { }
                }

                _connection = new HubConnectionBuilder()
                    .WithUrl(_navigationManager.BaseUri.TrimEnd('/') + GameHub.HubPath, options => {
                        var request = _httpContextAccessor.HttpContext.Request;
                        var cookies = request.Cookies;
                        foreach (var cookie in cookies)
                        {
                            options.Cookies.Add(new Cookie(cookie.Key, cookie.Value, "/", request.Host.Host));
                        }
                    })
                    .Build();

                _connection.Closed += Connection_Closed;

                await _connection.StartAsync();
            }
            finally
            {
                _connectionLock.Release();
            }
        }

        public async void Dispose()
        {
            _isDisposed = true;

            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }

            GC.SuppressFinalize(this);
        }

        private Task Connection_Closed(Exception arg)
        {
            if (!_isDisposed)
            {
                return Task.Run(async () => await Connect());
            }

            return Task.CompletedTask;
        }
    }
}
