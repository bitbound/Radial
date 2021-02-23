using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Radial.Enums;
using Radial.Services.Client;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IInputDispatcher
    {
        Task Attack(IClientConnection clientConnection);

        Task Blast(IClientConnection clientConnection);

        Task Block(IClientConnection clientConnection);

        Task EscapeCombat(IClientConnection clientConnection);
        Task Heal(IClientConnection clientConnection);

        Task MoveCharacter(IClientConnection clientConnection, MovementDirection direction);
        Task Respawn(IClientConnection clientConnection);

        void QueueInput(IClientConnection clientConnection, Action inputAction);
    }

    public class InputDispatcher : IInputDispatcher
    {
        private readonly ConcurrentDictionary<IClientConnection, Action> _inputs = 
            new ConcurrentDictionary<IClientConnection, Action>();
        private readonly ILocationService _locationService;
        private readonly ILogger<InputDispatcher> _logger;

        public InputDispatcher(
            ILocationService locationService,
            ILogger<InputDispatcher> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        public void QueueInput(IClientConnection clientConnection, Action inputAction)
        {
            _inputs.AddOrUpdate(clientConnection, inputAction, (k, v) => inputAction);
        }
        public void DispatchInputs()
        {
            foreach (var input in _inputs)
            {
                try
                {
                    if (_inputs.TryRemove(input.Key, out var result))
                    {
                        result.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error dispatching input.");
                }
            }   
        }
        public async Task Attack(IClientConnection clientConnection)
        {

        }

        public async Task Blast(IClientConnection clientConnection)
        {

        }

        public async Task Block(IClientConnection clientConnection)
        {

        }

        public async Task EscapeCombat(IClientConnection clientConnection)
        {

        }

        public async Task Heal(IClientConnection clientConnection)
        {

        }

        public async Task MoveCharacter(IClientConnection clientConnection, MovementDirection direction)
        {
            
        }

        public async Task Respawn(IClientConnection clientConnection)
        {

        }
    }
}
