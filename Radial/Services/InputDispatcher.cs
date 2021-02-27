using Microsoft.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Radial.Enums;
using Radial.Models;
using Radial.Models.Messaging;
using Radial.Services.Client;
using Radial.Utilities;
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

        void Guard(IClientConnection clientConnection);
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
        private readonly ICombatService _combatService;
        private readonly IClientManager _clientManager;
        private readonly IWorld _world;
        private readonly ILocationService _locationService;
        private readonly ILogger<InputDispatcher> _logger;

        public InputDispatcher(
            IWorld world,
            IClientManager clientManager,
            ILocationService locationService,
            ICombatService combatService,
            ILogger<InputDispatcher> logger)
        {
            _combatService = combatService;
            _clientManager = clientManager;
            _world = world;
            _locationService = locationService;
            _logger = logger;
        }

        public void Attack(IClientConnection clientConnection)
        {
            QueueInput(clientConnection, () =>
            {
                if (clientConnection.Character.ChargeCurrent < 1)
                {
                    clientConnection.InvokeMessageReceived(new LocalEventMessage("No charge available to perform action."));
                    return Task.CompletedTask;
                }
                _combatService.AttackTarget(clientConnection.Character, clientConnection.Location, clientConnection.Character.ActionBonus);
                return Task.CompletedTask;
            });
        }

        public void Blast(IClientConnection clientConnection)
        {
            QueueInput(clientConnection, () =>
            {
                if (clientConnection.Character.ChargeCurrent < 1)
                {
                    clientConnection.InvokeMessageReceived(new LocalEventMessage("No charge available to perform action."));
                    return Task.CompletedTask;
                }
                return Task.CompletedTask;
            });
        }

        public void Guard(IClientConnection clientConnection)
        {
            QueueInput(clientConnection, () =>
            {
                if (clientConnection.Character.ChargeCurrent < 1)
                {
                    clientConnection.InvokeMessageReceived(new LocalEventMessage("No charge available to perform action."));
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
            if (clientConnection.Character.ChargePercent < 1)
            {
                clientConnection.InvokeMessageReceived(new LocalEventMessage("You must full charge to escape!", "text-warning"));
                return;
            }

            _clientManager.SendToAllAtLocation(clientConnection.Location, 
                new LocalEventMessage($"{clientConnection.Character.Name} has teleported away!", "text-success"));

            clientConnection.Location.Characters.Remove(clientConnection.Character);
            _world.StartLocation.Characters.Add(clientConnection.Character);
            clientConnection.Location = _world.StartLocation;
            clientConnection.Character.ChargeCurrent = 0;
        }

        public void Heal(IClientConnection clientConnection)
        {
            QueueInput(clientConnection, () =>
            {
                if (clientConnection.Character.ChargeCurrent < 1)
                {
                    clientConnection.InvokeMessageReceived(new LocalEventMessage("No charge available to perform action."));
                    return Task.CompletedTask;
                }

                var target = clientConnection.Character.Target;

                if (target is Npc || target is null)
                {
                    target = clientConnection.Character;
                }

                _combatService.HealCharacter(clientConnection.Character,
                    target,
                    clientConnection.Location,
                    clientConnection.Character.ActionBonus,
                    clientConnection.Location.PlayersAlive.Except(new[] { clientConnection.Character }));

                if (clientConnection.Location.PlayersAlive.Any(x=>x.State == CharacterState.InCombat))
                {
                    clientConnection.Character.State = CharacterState.InCombat;
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
            _clientManager.SendToAllAtLocation(clientConnection.Location,
                new LocalEventMessage($"{clientConnection.Character.Name} respawned and teleported away!", "text-info"));

            clientConnection.Location.Characters.Remove(clientConnection.Character);
            _world.StartLocation.Characters.Add(clientConnection.Character);
            clientConnection.Location = _world.StartLocation;
            clientConnection.Character.State = CharacterState.Normal;
            clientConnection.Character.EnergyCurrent = (long)(clientConnection.Character.EnergyMax * .1);
        }

        private void QueueInput(IClientConnection clientConnection, Func<Task> inputAction)
        {
            _inputs.AddOrUpdate(clientConnection, inputAction, (k, v) => inputAction);
        }
    }
}
