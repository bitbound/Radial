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
        event EventHandler<ChatMessage> ChatReceived;
        event EventHandler DataStateChanged;
        event EventHandler<LocalEventMessage> LocalEventReceived;
        event EventHandler LocationChanged;
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

        public event EventHandler<ChatMessage> ChatReceived;
        public event EventHandler DataStateChanged;
        public event EventHandler LocationChanged;
        public event EventHandler<LocalEventMessage> LocalEventReceived;


        private void MessageReceived(object sender, IMessageBase message)
        {
            try
            {
                switch (message.MessageType)
                {
                    case MessageType.ChatMessage:
                        ChatReceived?.Invoke(this, message as ChatMessage);
                        break;
                    case MessageType.DataStateChanged:
                        DataStateChanged?.Invoke(this, null);
                        break;
                    case MessageType.LocalEvent:
                        LocalEventReceived?.Invoke(this, message as LocalEventMessage);
                        break;
                    case MessageType.LocationChanged:
                        LocationChanged?.Invoke(this, null);
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
