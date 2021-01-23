using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Radial.Models.Dtos;
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
        void RemoveClient(string connectionId);

        bool SendToClient(string recipient, IBaseDto dto);
    }

    public class ClientManager : IClientManager
    {
        public static ConcurrentDictionary<string, IClientConnection> ClientConnections { get; } =
            new ConcurrentDictionary<string, IClientConnection>();

        public void AddClient(string connectionId, IClientConnection clientConnection)
        {
            var existingConnection = ClientConnections.FirstOrDefault(x => x.Value.Username == clientConnection.Username);

            if (existingConnection.Value != null)
            {
                ClientConnections.Remove(existingConnection.Key, out _);
                existingConnection.Value.Disconnect("You've been disconnected because you signed in from another tab or browser.");
            }

            ClientConnections.AddOrUpdate(connectionId, clientConnection, (k, v) => clientConnection);

            foreach (var other in ClientConnections.Where(x => x.Key != connectionId))
            {
                other.Value.InvokeDtoReceived(new ChatMessageDto()
                {
                    Message = "Signed in.",
                    Sender = clientConnection.Username
                });
            }
        }

        public void RemoveClient(string connectionId)
        {
            if (ClientConnections.TryRemove(connectionId, out var connection))
            {
                connection.Disconnect("Session closed by server.");
            }
        }

        public bool SendToClient(string recipient, IBaseDto dto)
        {
            if (ClientConnections.TryGetValue(recipient, out var connection))
            {
                connection.InvokeDtoReceived(dto);
                return true;
            }
            return false;
        }
    }
}
