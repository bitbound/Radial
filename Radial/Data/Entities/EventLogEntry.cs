using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;

namespace Radial.Data.Entities
{
    public class EventLogEntry
    {
        [Key]
        public string ID { get; set; } = Guid.NewGuid().ToString();
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; }
        public string Source { get; set; }
        public string StackTrace { get; set; }
        public DateTimeOffset TimeStamp { get; set; } = DateTimeOffset.Now;
    }
}
