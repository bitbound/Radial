using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Radial.Services
{
    public class GameEngine : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameEngine> _logger;
        private CancellationToken _cancelToken;
        private DateTimeOffset _lastLoop = DateTimeOffset.Now;
        private readonly TimeSpan _desiredLoopTime = TimeSpan.FromMilliseconds(100);

        public GameEngine(IServiceProvider serviceProvider, ILogger<GameEngine> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _cancelToken = stoppingToken;
            return Task.Run(MainLoop, stoppingToken);
        }

        private async Task MainLoop()
        {
            using var _ = _logger.BeginScope(nameof(GameEngine));
            _logger.LogInformation("Main engine started.");

            while (!_cancelToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var world = scope.ServiceProvider.GetRequiredService<IWorld>();
                    var inputDispatcher = scope.ServiceProvider.GetRequiredService<IInputDispatcher>();
                    var loopSpeed = await GetSpeedMultiplier();
                    await inputDispatcher.DispatchInputs();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in main loop.");
                }
            }
        }

        private async Task<double> GetSpeedMultiplier()
        {
            var elapsed = DateTimeOffset.Now - _lastLoop;
            if (elapsed < _desiredLoopTime)
            {
                var waitTime = _desiredLoopTime - elapsed;
                Debug.WriteLine($"Waiting {waitTime} in main loop.");
                Debug.WriteLine($"Loop time is {elapsed}.");
                await Task.Delay(waitTime);
            }
            else
            {
                Debug.WriteLine($"Loop time is slow.  Elapsed: {elapsed}");
            }

            var speedMultiplier = (DateTimeOffset.Now - _lastLoop).TotalMilliseconds / _desiredLoopTime.TotalMilliseconds;

            _lastLoop = DateTimeOffset.Now;

            return speedMultiplier;
        }
    }
}
