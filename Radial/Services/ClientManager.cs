using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Radial.Data.Entities;
using Radial.Models.Messaging;
using Radial.Services.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IClientManager
    {
        Task AddClient(IClientConnection clientConnection);
        bool IsPlayerOnline(string username);
        void Broadcast(IMessageBase message);

        Task RemoveClient(IClientConnection clientConnection);

        bool SendToClient(IClientConnection senderConnection, string recipient, IMessageBase message, bool copyToSelf = false);
        void SendToLocal(IClientConnection senderConnection, IMessageBase message);

        bool SendToParty(IClientConnection senderConnection, IMessageBase message);

    }

    public class ClientManager : IClientManager
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDataService _dataService;

        public ClientManager(IServiceProvider serviceProvider, IDataService dataService)
        {
            _serviceProvider = serviceProvider;
            _dataService = dataService;
        }

        private static ConcurrentDictionary<string, IClientConnection> ClientConnections { get; } =
            new ConcurrentDictionary<string, IClientConnection>();

        public Task AddClient(IClientConnection clientConnection)
        {
            if (clientConnection?.User?.Character is null)
            {
                return Task.CompletedTask;
            }

            if (ClientConnections.TryRemove(clientConnection.User.Id, out var existingConnection))
            {
                existingConnection.Disconnect("You've been disconnected because you signed in from another tab or browser.");
            }

            ClientConnections.AddOrUpdate(clientConnection.User.Id, clientConnection, (k, v) => clientConnection);

            foreach (var other in ClientConnections.Where(x => x.Value.User.Id != clientConnection.User.Id))
            {
                other.Value.InvokeMessageReceived(new ChatMessage()
                {
                    Message = $"{clientConnection.User.UserName} has signed in.",
                    Sender = "System",
                    Channel = Enums.ChatChannel.System
                });

                if (other.Value.User.Character.Location == clientConnection.User.Character.Location)
                {
                    other.Value.InvokeMessageReceived(new GenericMessage()
                    {
                        MessageType = Models.Enums.MessageType.CharacterInfoUpdated
                    });
                }
            }
            return Task.CompletedTask;
        }

        public void Broadcast(IMessageBase message)
        {
            foreach (var clientConnection in ClientConnections.Values)
            {
                clientConnection.InvokeMessageReceived(message);
            };
        }

        public bool IsPlayerOnline(string username)
        {
            return ClientConnections.Values.Any(x => x.User.UserName.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public async Task RemoveClient(IClientConnection clientConnection)
        {
            var user = clientConnection.User;
            var location = clientConnection.Location;

            if (user?.Character is null)
            {
                return;
            }

            if (ClientConnections.TryRemove(user.Id, out _))
            {
                var locationXyz = user.Character.Location.XYZ;
                location.Characters.RemoveAll(x => x.Id == user.Character.Id);
                user.Character.Location = null;
                await _dataService.SaveEntity(user);
                await _dataService.SaveEntity(location);

                foreach (var other in ClientConnections.Where(x => x.Value.User.Id != user.Id))
                {
                    other.Value.InvokeMessageReceived(new ChatMessage()
                    {
                        Message = $"{user.UserName} has signed out.",
                        Sender = "System",
                        Channel = Enums.ChatChannel.System
                    });

                    if (other.Value.User.Character.XYZ == locationXyz)
                    {
                        other.Value.InvokeMessageReceived(new GenericMessage()
                        {
                            MessageType = Models.Enums.MessageType.CharacterInfoUpdated
                        });
                    }
                }

            }
        }

        public bool SendToClient(IClientConnection senderConnection, string recipient, IMessageBase dto, bool copyToSelf = false)
        {
            if (recipient is null)
            {
                return false;
            }

            var clientConnection = ClientConnections.Values.FirstOrDefault(x => 
                x.User.UserName.Equals(recipient?.Trim(), StringComparison.OrdinalIgnoreCase));

            if (clientConnection is null)
            {
                return false;
            }

            clientConnection.InvokeMessageReceived(dto);

            if (copyToSelf)
            {
                senderConnection.InvokeMessageReceived(dto);
            }
            return true;
        }

        public void SendToLocal(IClientConnection senderConnection, IMessageBase message)
        {
            foreach (var connection in ClientConnections.Values.Where(x => x.User.Character.XYZ == senderConnection.User.Character.XYZ))
            {
                connection.InvokeMessageReceived(message);
            }
        }

        public bool SendToParty(IClientConnection senderConnection, IMessageBase message)
        {
            if (string.IsNullOrWhiteSpace(senderConnection.User.Character.PartyId))
            {
                return false;
            }

            foreach (var connection in ClientConnections.Values.Where(x=>x.User.Character.PartyId == senderConnection.User.Character.PartyId))
            {
                connection.InvokeMessageReceived(message);
            }
            return true;
        }
    }
}
