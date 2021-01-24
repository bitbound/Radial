using Radial.Enums;
using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models.Messaging
{
    public class ChatMessage : IMessageBase
    {
        public ChatChannel Channel { get; init; }
        public string Message { get; init; }
        public MessageType MessageType => MessageType.ChatMessage;
        public string Recipient { get; init; }
        public string Sender { get; init; }
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
    }
}
