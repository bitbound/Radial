using Radial.Models.Enums;

namespace Radial.Models.Messaging
{
    public class PartyInvite : IMessageBase
    {
        public PartyInvite(PlayerCharacter from)
        {
            From = from;
        }

        public PlayerCharacter From { get; init; }
        public MessageType MessageType => MessageType.PartyInviteReceived;
    }
}
