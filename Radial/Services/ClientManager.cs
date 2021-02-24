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
        void SendToLocals(IClientConnection senderConnection, IMessageBase message);
        void SendToLocals(IClientConnection senderConnection, Location location, IMessageBase message);

        bool SendToParty(IClientConnection senderConnection, IMessageBase message);
        IEnumerable<IClientConnection> GetPartyMembers(IClientConnection clientConnection);
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
            var character = clientConnection.Character;
            var oldLocation = clientConnection.Location;
            var newLocation = _world.StartLocation;

            if (character is null)
            {
                return Task.CompletedTask;
            }

            if (ClientConnections.TryRemove(clientConnection.User.Id, out var existingConnection))
            {
                existingConnection.Disconnect("You've been disconnected because you signed in from another tab or browser.");
            }

            oldLocation.Characters.Remove(character);
            newLocation.Characters.Add(character);

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

            foreach (var other in newLocation.Players.Where(x => x.Name != character.Name))
            {
                var player = ClientConnections.Values.FirstOrDefault(x => x.Character.Name == other.Name);
                player.InvokeMessageReceived(new LocalEventMessage()
                {
                    Message = $"{character.Name} has appeared."
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

        public IEnumerable<IClientConnection> GetPartyMembers(IClientConnection clientConnection)
        {
            if (string.IsNullOrWhiteSpace(clientConnection?.Character?.PartyId))
            {
                return new IClientConnection[] { clientConnection };
            }

            return ClientConnections.Values.Where(x => x.Character.PartyId == clientConnection.Character.PartyId);
        }

        public bool IsPlayerOnline(string username)
        {
            return ClientConnections.Values.Any(x => x.User.UserName.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Task RemoveClient(IClientConnection clientConnection)
        {
            var character = clientConnection.Character;
            var location = clientConnection.Location;

            if (character is null)
            {
                return Task.CompletedTask;
            }

            _world.CharacterBackups.AddOrUpdate(character.Name, character);

            if (ClientConnections.TryRemove(character.UserId, out _))
            {
                var locationXyz = location.XYZ;
       
                location.Characters.Remove(character);
                _world.OfflineLocation.Characters.Add(character);


                foreach (var other in ClientConnections.Where(x => x.Value.User.Id != character.UserId))
                {
                    other.Value.InvokeMessageReceived(new ChatMessage()
                    {
                        Message = $"{character.Name} has signed out.",
                        Sender = "System",
                        Channel = Enums.ChatChannel.System
                    });
                }

                foreach (var other in location.Players.Where(x => x.Name != clientConnection.Character.Name))
                {
                    var player = ClientConnections.Values.FirstOrDefault(x => x.Character.Name == other.Name);
                    player.InvokeMessageReceived(new LocalEventMessage()
                    {
                        Message = $"{character.Name} has disappeared."
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

        public void SendToLocals(IClientConnection senderConnection, IMessageBase message)
        {
            foreach (var connection in GetLocalConnections(senderConnection))
            {
                connection.InvokeMessageReceived(message);
            }
        }

        public void SendToLocals(IClientConnection senderConnection, Location location, IMessageBase message)
        {
            var connections = location.Players
                .Where(x => x.Name != senderConnection.Character.Name)
                .Select(x => ClientConnections.Values.FirstOrDefault(y => y.Character.Name == x.Name));

            foreach (var connection in connections)
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
                .Where(x => x.Name != senderConnection.Character.Name)
                .Select(x => ClientConnections.Values.FirstOrDefault(y => y.Character.Name == x.Name));
        }
    }
}
