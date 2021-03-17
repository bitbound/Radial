using Microsoft.Extensions.Logging;
using Radial.Models.Enums;
using Radial.Models.Messaging;
using System;

namespace Radial.Services.Client
{
    public interface IMessagePublisher
    {
        event EventHandler<ChatMessage> ChatReceived;
        event EventHandler DataStateChanged;
        event EventHandler<LocalEventMessage> LocalEventReceived;
        event EventHandler LocationChanged;
        event EventHandler<PartyInvite> PartyInviteReceived;
    }
    public class MessagePublisher : IMessagePublisher
    {
        private readonly IClientConnection _clientConnection;
        private readonly IToastService _toastService;
        private readonly ILogger<MessagePublisher> _logger;

        public MessagePublisher(IClientConnection clientConnection, IToastService toastService, ILogger<MessagePublisher> logger)
        {
            _clientConnection = clientConnection;
            _clientConnection.MessageReceived += MessageReceived;
            _toastService = toastService;
            _logger = logger;
        }

        public event EventHandler<ChatMessage> ChatReceived;
        public event EventHandler DataStateChanged;
        public event EventHandler LocationChanged;
        public event EventHandler<LocalEventMessage> LocalEventReceived;
        public event EventHandler<PartyInvite> PartyInviteReceived;


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
                        DataStateChanged?.Invoke(this, EventArgs.Empty);
                        break;
                    case MessageType.LocalEvent:
                        LocalEventReceived?.Invoke(this, message as LocalEventMessage);
                        break;
                    case MessageType.LocationChanged:
                        LocationChanged?.Invoke(this, EventArgs.Empty);
                        break;
                    case MessageType.PartyInviteReceived:
                        var invite = message as PartyInvite;
                        _toastService.ShowToast($"Party invite from {invite.From.Name}.");
                        PartyInviteReceived?.Invoke(this, invite);
                        break;
                    case MessageType.ToastMessage:
                        if (message is ToastMessage toastMessage)
                        {
                            _toastService.ShowToast(toastMessage.Message, toastMessage.ExpirationMs, toastMessage.ClassString, toastMessage.StyleOverride);
                        }
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
