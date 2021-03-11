using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
