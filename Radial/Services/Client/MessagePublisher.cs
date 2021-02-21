using Microsoft.Extensions.Logging;
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
        event EventHandler CharacterInfoChanged;
        event EventHandler<ChatMessage> ChatReceived;
    }
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IClientConnection _clientConnection;
        private readonly ILogger<MessagePublisher> _logger;

        public MessagePublisher(IClientConnection clientConnection, ILogger<MessagePublisher> logger)
        {
            _clientConnection = clientConnection;
            _clientConnection.MessageReceived += MessageReceived;
            _logger = logger;
        }

        public event EventHandler CharacterInfoChanged;
        public event EventHandler<ChatMessage> ChatReceived;

        private void MessageReceived(object sender, IMessageBase message)
        {
            try
            {
                switch (message.MessageType)
                {
                    case MessageType.ChatMessage:
                        ChatReceived?.Invoke(this, message as ChatMessage);
                        break;
                    case MessageType.CharacterInfoUpdated:
                        CharacterInfoChanged?.Invoke(this, null);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while processing message.");
            }
  
        }
    }
}
