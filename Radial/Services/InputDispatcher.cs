using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Radial.Enums;
using Radial.Models.Messaging;
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
        void Attack(IClientConnection clientConnection);

        void Blast(IClientConnection clientConnection);

        void Block(IClientConnection clientConnection);
        Task DispatchInputs();
        void EscapeCombat(IClientConnection clientConnection);
        void Heal(IClientConnection clientConnection);

        void MoveCharacter(IClientConnection clientConnection, MovementDirection direction);

        void Respawn(IClientConnection clientConnection);
    }

    public class InputDispatcher : IInputDispatcher
    {
        private static readonly ConcurrentDictionary<IClientConnection, Func<Task>> _inputs = 
            new ConcurrentDictionary<IClientConnection, Func<Task>>();
        private readonly ILocationService _locationService;
        private readonly ILogger<InputDispatcher> _logger;

        public InputDispatcher(
            ILocationService locationService,
            ILogger<InputDispatcher> logger)
        {
            _locationService = locationService;
            _logger = logger;
        }

        public void Attack(IClientConnection clientConnection)
        {
            QueueInput(clientConnection, () =>
            {
                if (clientConnection.Character.ChargeCurrent < 1)
                {
                    clientConnection.InvokeMessageReceived(new LocalEventMessage()
                    {
                        Message = "No charge available to perform action."
                    });
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            });
        }

        public void Blast(IClientConnection clientConnection)
        {
            QueueInput(clientConnection, () =>
            {
                if (clientConnection.Character.ChargeCurrent < 1)
                {
                    clientConnection.InvokeMessageReceived(new LocalEventMessage()
                    {
                        Message = "No charge available to perform action."
                    });
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            });
        }

        public void Block(IClientConnection clientConnection)
        {
            QueueInput(clientConnection, () =>
            {
                if (clientConnection.Character.ChargeCurrent < 1)
                {
                    clientConnection.InvokeMessageReceived(new LocalEventMessage()
                    {
                        Message = "No charge available to perform action."
                    });
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            });
        }

        public async Task DispatchInputs()
        {
            foreach (var input in _inputs)
            {
                try
                {
                    if (_inputs.TryRemove(input.Key, out var result))
                    {
                        await result.Invoke();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error dispatching input.");
                }
            }
        }

        public void EscapeCombat(IClientConnection clientConnection)
        {

        }

        public void Heal(IClientConnection clientConnection)
        {
            QueueInput(clientConnection, () =>
            {
                if (clientConnection.Character.ChargeCurrent < 1)
                {
                    clientConnection.InvokeMessageReceived(new LocalEventMessage()
                    {
                        Message = "No charge available to perform action."
                    });
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            });
        }

        public void MoveCharacter(IClientConnection clientConnection, MovementDirection direction)
        {
            QueueInput(clientConnection, () =>
            {
                _locationService.MoveCharacter(clientConnection, direction);
                return Task.CompletedTask;
            });
        }

        public void Respawn(IClientConnection clientConnection)
        {

        }

        private void QueueInput(IClientConnection clientConnection, Func<Task> inputAction)
        {
            _inputs.AddOrUpdate(clientConnection, inputAction, (k, v) => inputAction);
        }
    }
}
