using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Radial.Data.Entities;
using Radial.Models;
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
        private readonly IWorld _world;

        public ClientManager(IWorld world)
        {
            _world = world;
        }

        private static ConcurrentDictionary<string, IClientConnection> ClientConnections { get; } =
            new ConcurrentDictionary<string, IClientConnection>();

        public Task AddClient(IClientConnection clientConnection)
        {
            if (clientConnection?.Character is null)
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
            }

            var location = clientConnection.Location;
            foreach (var other in location.Players.Where(x => x.Id != clientConnection.Character.Id))
            {
                var player = ClientConnections.Values.FirstOrDefault(x => x.Character.Id == other.Id);
                player.InvokeMessageReceived(new GenericMessage()
                {
                    MessageType = Models.Enums.MessageType.CharacterInfoUpdated
                });
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

        public Task RemoveClient(IClientConnection clientConnection)
        {
            var user = clientConnection.User;
            var character = clientConnection.Character;
            var location = clientConnection.Location;

            if (character is null)
            {
                return Task.CompletedTask;
            }

            if (ClientConnections.TryRemove(user.Id, out _))
            {
                var locationXyz = location.XYZ;
                var purgatory = _world.PurgatoryLocation;

                location.Characters.RemoveAll(x => x.Id == character.Id);
                purgatory.Characters.Add(character);


                foreach (var other in ClientConnections.Where(x => x.Value.User.Id != user.Id))
                {
                    other.Value.InvokeMessageReceived(new ChatMessage()
                    {
                        Message = $"{user.UserName} has signed out.",
                        Sender = "System",
                        Channel = Enums.ChatChannel.System
                    });
                }

                foreach (var other in location.Players.Where(x => x.Id != clientConnection.Character.Id))
                {
                    var player = ClientConnections.Values.FirstOrDefault(x => x.Character.Id == other.Id);
                    player.InvokeMessageReceived(new GenericMessage()
                    {
                        MessageType = Models.Enums.MessageType.CharacterInfoUpdated
                    });
                }
            }

            return Task.CompletedTask;
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
            foreach (var connection in GetLocalConnections(senderConnection))
            {
                connection.InvokeMessageReceived(message);
            }
        }

        public bool SendToParty(IClientConnection senderConnection, IMessageBase message)
        {
            if (string.IsNullOrWhiteSpace(senderConnection.Character.PartyId))
            {
                return false;
            }

            foreach (var connection in ClientConnections.Values.Where(x=>x.Character.PartyId == senderConnection.Character.PartyId))
            {
                connection.InvokeMessageReceived(message);
            }
            return true;
        }

        private IEnumerable<IClientConnection> GetLocalConnections(IClientConnection senderConnection)
        {
            var location = senderConnection.Location;
            return location.Players
                .Where(x => x.Id != senderConnection.Character.Id)
                .Select(x => ClientConnections.Values.FirstOrDefault(y => y.Character.Id == x.Id));
        }
    }
}
