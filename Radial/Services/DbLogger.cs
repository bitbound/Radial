using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Radial.Data;
using Radial.Data.Entities;
using System;
using System.Collections.Concurrent;
using System.Threading;

namespace Radial.Services
{
    public class DbLogger : ILogger
    {
        private readonly string _categoryName;
        private readonly IWebHostEnvironment _hostEnvironment;
        private readonly IServiceProvider _serviceProvider;

        protected static ConcurrentStack<string> ScopeStack { get; } = new ConcurrentStack<string>();

        public DbLogger(string categoryName, IWebHostEnvironment hostEnvironment, IServiceProvider serviceProvider)
        {
            _categoryName = categoryName;
            _hostEnvironment = hostEnvironment;
            _serviceProvider = serviceProvider;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            ScopeStack.Push(state.ToString());
            return new NoopDisposable();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    break;
                case LogLevel.Debug:
                case LogLevel.Information:
                    if (_hostEnvironment.IsDevelopment())
                    {
                        return true;
                    }
                    break;
                case LogLevel.Warning:
                case LogLevel.Error:
                case LogLevel.Critical:
                    return true;
                case LogLevel.None:
                    break;
                default:
                    break;
            }
            return false;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                // Prevent re-entrancy.
                if (eventId.Name?.Contains("EntityFrameworkCore") == true)
                {
                    return;
                }

                dbContext.EventLogs.Add(new EventLogEntry()
                {
                    StackTrace = exception?.StackTrace,
                    LogLevel = logLevel,
                    Message = $"[{logLevel}] [{string.Join(" - ", ScopeStack)} - {_categoryName}] | Message: {state} | Exception: {exception?.Message}",
                    TimeStamp = DateTimeOffset.Now
                });

                dbContext.SaveChanges();
            }
            catch { }
        }


        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
                while (!ScopeStack.TryPop(out _))
                {
                    Thread.Sleep(100);
                }
            }
        }
    }
}
