using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models.Enums
{
    public enum MessageType
    {
        ChatMessage,
        DataStateChanged,
        LocalEvent,
        LocationChanged,
        PartyInviteReceived,
        ToastMessage
    }
}
