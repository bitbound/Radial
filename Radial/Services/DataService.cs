using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IDataService
    {
        Task WriteLog(LogLevel logLevel, string categoryName, EventId eventId, string state, Exception exception, List<string> scopeStack);
        Task WriteLog(LogLevel logLevel, string categoryName, EventId eventId, string state, Exception exception, List<string> scopeStack, string projectId);
    }

    public class DataService : IDataService
    {
        public async Task WriteLog(LogLevel logLevel, string categoryName, EventId eventId, string state, Exception exception, List<string> lists)
        {
            // TODO
            await Task.Delay(0);
        }

        public async Task WriteLog(LogLevel logLevel, string categoryName, EventId eventId, string state, Exception exception, List<string> scopeStack, string projectId)
        {
            // TODO
            await Task.Delay(0);
        }
    }
}
