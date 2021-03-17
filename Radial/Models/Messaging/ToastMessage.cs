using Radial.Models.Enums;

namespace Radial.Models.Messaging
{
    public class ToastMessage : IMessageBase
    {
        public ToastMessage(string message, string classString = null, string styleOverride = null, int timeoutMs = 3000)
        {
            Message = message;
            ClassString = classString;
            StyleOverride = styleOverride;
            ExpirationMs = timeoutMs;
        }

        public string Message { get; init; }
        public string ClassString { get; set; }
        public string StyleOverride { get; set; }
        public int ExpirationMs { get; set; }
        public MessageType MessageType => MessageType.ToastMessage;
    }
}
