using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models.Messaging
{
    public class GenericMessage : IMessageBase
    {
        public static GenericMessage StateChanged { get; } = 
            new GenericMessage() { MessageType = MessageType.DataStateChanged };

        public static GenericMessage LocationChanged { get; } = 
            new GenericMessage() { MessageType = MessageType.LocationChanged };

        public MessageType MessageType { get; set; }
    }
}
