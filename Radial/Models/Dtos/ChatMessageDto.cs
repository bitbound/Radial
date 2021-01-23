using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models.Dtos
{
    public class ChatMessageDto : IBaseDto
    {
        public string Sender { get; init; }
        public string Message { get; init; }
        public DtoType DtoType => DtoType.ChatMessage;
    }
}
