using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models.Messaging
{
    public class GenericMessage : IMessageBase
    {
        public MessageType MessageType { get; set; }
    }
}
