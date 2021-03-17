using Radial.Models.Enums;

namespace Radial.Models.Messaging
{
    public interface IMessageBase
    {
        public MessageType MessageType { get; }
    }
}
