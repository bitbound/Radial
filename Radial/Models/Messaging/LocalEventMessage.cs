using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models.Messaging
{
    public class LocalEventMessage : IMessageBase
    {
        public string Message { get; init; }
        public MessageType MessageType => MessageType.LocalEvent;
        public DateTimeOffset Timestamp { get; } = DateTimeOffset.Now;
    }
}
