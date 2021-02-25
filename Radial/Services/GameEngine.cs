using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Radial.Models.Messaging;
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
        private readonly IWorld _world;
        private readonly IClientManager _clientManager;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<GameEngine> _logger;
        private CancellationToken _cancelToken;

        private DateTimeOffset _lastOneSecondTick = DateTimeOffset.Now;
        private DateTimeOffset _lastLoop = DateTimeOffset.Now;
        private readonly TimeSpan _desiredLoopTime = TimeSpan.FromMilliseconds(100);

        public GameEngine(IWorld world, IClientManager clientManager, IServiceProvider serviceProvider, ILogger<GameEngine> logger)
        {
            _world = world;
            _clientManager = clientManager;
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
                    var timeMultiplier = await GetTimeMultiplier();

                    using var scope = _serviceProvider.CreateScope();
                 
                    await RunImmediateActions(scope);
                    await RunOneSecondActions(scope);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in main loop.");
                }
            }
        }

        private Task RunOneSecondActions(IServiceScope scope)
        {
            if (DateTimeOffset.Now - _lastOneSecondTick < TimeSpan.FromSeconds(1))
            {
                return Task.CompletedTask;
            }

            _lastOneSecondTick = DateTimeOffset.Now;

            foreach (var client in _clientManager.Clients)
            {
                if (client.Character.ChargeCurrent != client.Character.ChargeMax)
                {
                    client.Character.ChargeCurrent = (long)Math.Max(0,
                        Math.Min(
                            client.Character.ChargeMax,
                            client.Character.ChargeCurrent + client.Character.ChargeRate * .2)
                    );
                    client.InvokeMessageReceived(GenericMessage.StateChanged);
                }
            }

            return Task.CompletedTask;
        }

        private async Task RunImmediateActions(IServiceScope scope)
        {
            var inputDispatcher = scope.ServiceProvider.GetRequiredService<IInputDispatcher>();
            await inputDispatcher.DispatchInputs();
        }

        private async Task<double> GetTimeMultiplier()
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
