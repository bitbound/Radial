using Radial.Data.Entities;
using Radial.Models.Enums;
using Radial.Models.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services.Client
{
    public interface IMessagePublisher
    {
        event EventHandler CharacterStatsChanged;
        event EventHandler<ChatMessage> ChatReceived;
    }
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IClientConnection _clientConnection;

        public event EventHandler CharacterStatsChanged;
        public event EventHandler<ChatMessage> ChatReceived;

        public MessagePublisher(IClientConnection clientConnection)
        {
            _clientConnection = clientConnection;
            _clientConnection.MessageReceived += MessageReceived;
        }

        private void MessageReceived(object sender, IMessageBase message)
        {
            switch (message.MessageType)
            {
                case MessageType.ChatMessage:
                    ChatReceived?.Invoke(this, message as ChatMessage);
                    break;
                case MessageType.CharacterStatsUpdated:
                    CharacterStatsChanged?.Invoke(this, null);
                    break;
                default:
                    break;
            }
        }
    }
}
