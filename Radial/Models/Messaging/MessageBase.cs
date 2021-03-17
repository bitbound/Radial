using Radial.Models.Enums;

namespace Radial.Models.Messaging
{
    public class MessageBase : IMessageBase
    {
        public static MessageBase StateChanged { get; } = 
            new MessageBase() { MessageType = MessageType.DataStateChanged };

        public static MessageBase LocationChanged { get; } = 
            new MessageBase() { MessageType = MessageType.LocationChanged };

        public MessageType MessageType { get; set; }
    }
}
