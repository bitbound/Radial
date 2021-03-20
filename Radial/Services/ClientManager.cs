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
        ICollection<IClientConnection> Clients { get; }

        Task AddClient(IClientConnection clientConnection);
        void Broadcast(IMessageBase message);

        IClientConnection FindConnection(PlayerCharacter member);

        IEnumerable<IClientConnection> GetPartyMembers(IClientConnection clientConnection);

        bool IsPlayerOnline(string username);
        Task RemoveClient(IClientConnection clientConnection);
        void SendToAllAtLocation(Location location, IMessageBase message);
        bool SendToClient(IClientConnection senderConnection, string recipient, IMessageBase message, bool copyToSelf = false);
        void SendToOtherLocals(IClientConnection senderConnection, IMessageBase message);
        void SendToOtherLocals(IClientConnection senderConnection, Location location, IMessageBase message);

        bool SendToParty(IClientConnection senderConnection, IMessageBase message);
        void SendToPlayer(PlayerCharacter player, IMessageBase localEventMessage);
    }

    public class ClientManager : IClientManager
    {
        private readonly static ConcurrentDictionary<string, IClientConnection> _clientConnections = new();

        private readonly IWorld _world;

        public ClientManager(IWorld world)
        {
            _world = world;
        }

        public ICollection<IClientConnection> Clients => _clientConnections.Values;

        public Task AddClient(IClientConnection clientConnection)
        {
            var character = clientConnection.Character;

            if (character is null)
            {
                return Task.CompletedTask;
            }

            if (_clientConnections.TryRemove(clientConnection.User.Id, out var existingConnection))
            {
                existingConnection.Disconnect("You've been disconnected because you signed in from another tab or browser.");
            }

            if (!string.IsNullOrWhiteSpace(character.LastLocation))
            {
                var lastLocation = _world.Locations.Get(character.LastLocation);
                clientConnection.Location = lastLocation;
            }

            clientConnection.Location.AddCharacter(character);

            _clientConnections.AddOrUpdate(clientConnection.User.Id, clientConnection, (k, v) => clientConnection);

            foreach (var other in _clientConnections.Where(x => x.Value.User.Id != clientConnection.User.Id))
            {
                other.Value.InvokeMessageReceived(new ChatMessage()
                {
                    Message = $"{clientConnection.User.UserName} has signed in.",
                    Sender = "System",
                    Channel = Enums.ChatChannel.System
                });
            }

            foreach (var other in clientConnection.Location.Players.Where(x => x.Name != character.Name))
            {
                var player = _clientConnections.Values.FirstOrDefault(x => x.Character.Name == other.Name);
                player?.InvokeMessageReceived(new LocalEventMessage($"{character.Name} has appeared."));
            }
            return Task.CompletedTask;
        }

        public void Broadcast(IMessageBase message)
        {
            foreach (var clientConnection in _clientConnections.Values)
            {
                clientConnection.InvokeMessageReceived(message);
            };
        }

        public IClientConnection FindConnection(PlayerCharacter member)
        {
            return _clientConnections.Values.FirstOrDefault(x => x.Character == member);
        }

        public IEnumerable<IClientConnection> GetPartyMembers(IClientConnection clientConnection)
        {
            if (clientConnection?.Character?.Party is null)
            {
                return new IClientConnection[] { clientConnection };
            }

            return _clientConnections.Values.Where(x => x.Character.Party == clientConnection.Character.Party);
        }

        public bool IsPlayerOnline(string username)
        {
            return _clientConnections.Values.Any(x => x.User.UserName.Equals(username.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        public Task RemoveClient(IClientConnection clientConnection)
        {
            var character = clientConnection.Character;
            var location = clientConnection.Location;

            if (character is null)
            {
                return Task.CompletedTask;
            }

            if (character.Party != null)
            {
                character.Party.Members.RemoveAll(x => x.Name == character.Name);
                character.Party = null;
                character.PartyInvites.Clear();
            }

            _world.CharacterBackups.AddOrUpdate(character.Name, character);

            if (_clientConnections.TryRemove(character.UserId, out _))
            {
                location.RemoveCharacter(character);

                foreach (var other in _clientConnections.Where(x => x.Value.User.Id != character.UserId))
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
                    var player = _clientConnections.Values.FirstOrDefault(x => x.Character.Name == other.Name);
                    player.InvokeMessageReceived(new LocalEventMessage($"{character.Name} has disappeared."));
                }
            }

            return Task.CompletedTask;
        }

        public void SendToAllAtLocation(Location location, IMessageBase message)
        {
            var connections = location.Players
                .Select(x => _clientConnections.Values.FirstOrDefault(y => y.Character.Name == x.Name));

            foreach (var connection in connections)
            {
                connection.InvokeMessageReceived(message);
            }
        }

        public bool SendToClient(IClientConnection senderConnection, string recipient, IMessageBase dto, bool copyToSelf = false)
        {
            if (recipient is null)
            {
                return false;
            }

            var clientConnection = _clientConnections.Values.FirstOrDefault(x => 
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

        public void SendToOtherLocals(IClientConnection senderConnection, IMessageBase message)
        {
            foreach (var connection in GetLocalConnections(senderConnection))
            {
                connection.InvokeMessageReceived(message);
            }
        }

        public void SendToOtherLocals(IClientConnection senderConnection, Location location, IMessageBase message)
        {
            var connections = location.Players
                .Where(x => x.Name != senderConnection.Character.Name)
                .Select(x => _clientConnections.Values.FirstOrDefault(y => y.Character.Name == x.Name));

            foreach (var connection in connections)
            {
                connection.InvokeMessageReceived(message);
            }
        }
        public bool SendToParty(IClientConnection senderConnection, IMessageBase message)
        {
            if (senderConnection.Character.Party is null)
            {
                return false;
            }

            foreach (var connection in _clientConnections.Values.Where(x=>x.Character.Party == senderConnection.Character.Party))
            {
                connection.InvokeMessageReceived(message);
            }
            return true;
        }

        public void SendToPlayer(PlayerCharacter player, IMessageBase localEventMessage)
        {
            var connection = _clientConnections.FirstOrDefault(x => x.Value.Character == player);

            if (connection.Value is null)
            {
                return;
            }

            connection.Value.InvokeMessageReceived(localEventMessage);
        }

        private IEnumerable<IClientConnection> GetLocalConnections(IClientConnection senderConnection)
        {
            var location = senderConnection.Location;
            return location.Players
                .Where(x => x.Name != senderConnection.Character.Name)
                .Select(x => _clientConnections.Values.FirstOrDefault(y => y.Character.Name == x.Name));
        }
    }
}
