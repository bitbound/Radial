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
        void Broadcast(IMessageBase message);

        void RemoveClient(string connectionId);

        bool SendToClient(string recipient, IMessageBase message);
    }

    public class ClientManager : IClientManager
    {
        public static ConcurrentDictionary<string, IClientConnection> ClientConnections { get; } =
            new ConcurrentDictionary<string, IClientConnection>();

        public void AddClient(string connectionId, IClientConnection clientConnection)
        {
            if (string.IsNullOrWhiteSpace(connectionId))
            {
                return;
            }

            var existingConnection = ClientConnections.FirstOrDefault(x => x.Value.Username == clientConnection.Username);

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
                    Message = $"{clientConnection.Username} has signed in.",
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

        public bool SendToClient(string recipient, IMessageBase dto)
        {
            if (ClientConnections.TryGetValue(recipient, out var connection))
            {
                connection.InvokeMessageReceived(dto);
                return true;
            }
            return false;
        }
    }
}
