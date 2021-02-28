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
        private readonly IClientManager _clientManager;
        private readonly TimeSpan _desiredLoopTime = TimeSpan.FromMilliseconds(100);
        private readonly ILogger<GameEngine> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IWorld _world;
        private CancellationToken _cancelToken;

        private TimeSpan _elapsed;
        private DateTimeOffset _lastLoop = DateTimeOffset.Now;
        private DateTimeOffset _lastOneSecondTick = DateTimeOffset.Now;
        private DateTimeOffset _lastThreeSecondTick;
        private double _timeMultiplier;
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

        private async Task CalculateElapsedTime()
        {
            _elapsed = DateTimeOffset.Now - _lastLoop;
            if (_elapsed < _desiredLoopTime)
            {
                var waitTime = _desiredLoopTime - _elapsed;
                Debug.WriteLine($"Waiting {waitTime} in main loop.");
                Debug.WriteLine($"Loop time is {_elapsed}.");
                await Task.Delay(waitTime);
            }
            else
            {
                Debug.WriteLine($"Loop time is slow.  Elapsed: {_elapsed}");
            }


            _elapsed = DateTimeOffset.Now - _lastLoop;
            _timeMultiplier = _elapsed.TotalMilliseconds / _desiredLoopTime.TotalMilliseconds;
            _lastLoop = DateTimeOffset.Now;
        }

        private async Task MainLoop()
        {
            using var _ = _logger.BeginScope(nameof(GameEngine));
            _logger.LogInformation("Main engine started.");

            while (!_cancelToken.IsCancellationRequested)
            {
                try
                {
                    await CalculateElapsedTime();

                    using var scope = _serviceProvider.CreateScope();
                 
                    await RunImmediateActions(scope);
                    await RunOneSecondActions(scope);
                    await RunThreeSecondActions(scope);
                    SendStateUpdates();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in main loop.");
                }
            }
        }

        private async Task RunImmediateActions(IServiceScope scope)
        {
            var inputDispatcher = scope.ServiceProvider.GetRequiredService<IInputDispatcher>();

            await inputDispatcher.DispatchInputs();
        }

        private Task RunOneSecondActions(IServiceScope scope)
        {
            if (DateTimeOffset.Now - _lastOneSecondTick < TimeSpan.FromSeconds(1))
            {
                return Task.CompletedTask;
            }

            var effectsService = scope.ServiceProvider.GetRequiredService<ICharacterEffectsService>();
            var encounterService = scope.ServiceProvider.GetRequiredService<IEncounterService>();
            var combatService = scope.ServiceProvider.GetRequiredService<ICombatService>();
            var locations = _clientManager.Clients.Select(x => x.Location).Distinct();

            foreach (var client in _clientManager.Clients)
            {
                encounterService.SpawnNpcCombatEncounters(client);
            }

            foreach (var location in locations)
            {
                effectsService.ApplyChargeRecovery(location, _timeMultiplier);
                combatService.EvaluateCombatStates(location);
            }

            _lastOneSecondTick = DateTimeOffset.Now;

            return Task.CompletedTask;
        }

        private Task RunThreeSecondActions(IServiceScope scope)
        {
            if (DateTimeOffset.Now - _lastThreeSecondTick < TimeSpan.FromSeconds(3))
            {
                return Task.CompletedTask;
            }

            var combatService = scope.ServiceProvider.GetRequiredService<ICombatService>();

            var locations =_clientManager.Clients.Select(x => x.Location).Distinct();

            foreach (var location in locations)
            {
                combatService.InitiateNpcAttackOnSight(location);
                combatService.ExecuteNpcActions(location);
            }

            _lastThreeSecondTick = DateTimeOffset.Now;

            return Task.CompletedTask;
        }
        private void SendStateUpdates()
        {
            foreach (var client in _clientManager.Clients)
            {
                client.InvokeMessageReceived(GenericMessage.StateChanged);
            }
        }
    }
}
