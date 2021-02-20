using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
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
        void AddClient(string connectionId, IClientConnection clientConnection);
        bool IsPlayerOnline(string username);
        void Broadcast(IMessageBase message);

        void RemoveClient(string connectionId);

        bool SendToClient(IClientConnection senderConnection, string recipient, IMessageBase message, bool copyToSelf = false);
        void SendToLocal(IClientConnection senderConnection, IMessageBase message);

        bool SendToParty(IClientConnection senderConnection, IMessageBase message);

    }

    public class ClientManager : IClientManager
    {
        private static ConcurrentDictionary<string, IClientConnection> ClientConnections { get; } =
            new ConcurrentDictionary<string, IClientConnection>();

        public void AddClient(string connectionId, IClientConnection clientConnection)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return;
            }

            var existingConnection = ClientConnections.FirstOrDefault(x => x.Value?.User?.UserName == clientConnection.User.UserName);

            if (existingConnection.Value != null)
            {
                ClientConnections.Remove(existingConnection.Key, out _);
                existingConnection.Value.Disconnect("You've been disconnected because you signed in from another tab or browser.");
            }

            ClientConnections.AddOrUpdate(connectionId, clientConnection, (k, v) => clientConnection);

            foreach (var other in ClientConnections.Where(x => x.Key != connectionId))
            {
                other.Value.InvokeMessageReceived(new ChatMessage()
                {
                    Message = $"{clientConnection.User.UserName} has signed in.",
                    Sender = "System",
                    Channel = Enums.ChatChannel.System
                });
            }
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

        public void RemoveClient(string connectionId)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return;
            }

            if (ClientConnections.TryRemove(connectionId, out var connection))
            {
                connection.Disconnect("Session closed by server.");
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
