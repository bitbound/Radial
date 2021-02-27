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
    public interface ILocationService
    {
        void MoveCharacter(IClientConnection clientConnection, MovementDirection direction);
    }

    public class LocationService : ILocationService
    {
        private readonly IClientManager _clientManager;
        private readonly ICombatService _combatService;
        private readonly INpcService _npcService;
        private readonly IWorld _world;
        public LocationService(
            IWorld world, 
            IClientManager clientManager, 
            ICombatService combatService, 
            INpcService npcService)
        {
            _world = world;
            _clientManager = clientManager;
            _combatService = combatService;
            _npcService = npcService;
        }


        public static void AddLogicalExits(ConcurrentList<MovementDirection> exits, IWorld world, string newXyz)
        {
            var (x, y, z) = ParseXyz(newXyz);

            if (world.Locations.Exists($"{x - 1},{y},{z}") &&
                !exits.Contains(MovementDirection.West))
            {
                exits.Add(MovementDirection.West);
            }
            if (world.Locations.Exists($"{x + 1},{y},{z}") &&
                !exits.Contains(MovementDirection.East))
            {
                exits.Add(MovementDirection.East);
            }
            if (world.Locations.Exists($"{x},{y - 1},{z}") &&
                !exits.Contains(MovementDirection.North))
            {
                exits.Add(MovementDirection.North);
            }
            if (world.Locations.Exists($"{x},{y + 1},{z}") &&
                !exits.Contains(MovementDirection.South))
            {
                exits.Add(MovementDirection.South);
            }
        }

        // TODO: Decide whether to add randomness to exits on world map.
        public static ConcurrentList<MovementDirection> GetOpenWorldExits(IWorld world, string newXyz, MovementDirection enterDirection)
        {
            var exits = new ConcurrentList<MovementDirection>();

            switch (enterDirection)
            {
                case MovementDirection.None:
                    return exits;
                case MovementDirection.North:
                case MovementDirection.East:
                case MovementDirection.South:
                case MovementDirection.West:
                    exits.Add(GetOppositeDirection(enterDirection));
                    break;
                default:
                    break;
            }

            AddLogicalExits(exits, world, newXyz);

            var allDirections = Enum.GetValues(typeof(MovementDirection))
                .OfType<MovementDirection>()
                .ToArray();

            for (var i = 0; i < allDirections.Length; i++)
            {
                var direction = allDirections[i];
                if (direction == MovementDirection.None ||
                    exits.Contains(direction))
                {
                    continue;
                }

                // Increase the odds of generating an exit in the direction you're already traveling.
                var odds = enterDirection == direction ?
                    .8 :
                    .5;

                var addExit = Calculator.RollForBool(odds);
                if (addExit)
                {
                    exits.Add(direction);
                }
            }
            return exits;
        }

        public static MovementDirection GetOppositeDirection(MovementDirection direction)
        {
            switch (direction)
            {
                case MovementDirection.None:
                    return MovementDirection.None;
                case MovementDirection.North:
                    return MovementDirection.South;
                case MovementDirection.East:
                    return MovementDirection.West;
                case MovementDirection.South:
                    return MovementDirection.North;
                case MovementDirection.West:
                    return MovementDirection.East;
                default:
                    return MovementDirection.None;
            }
        }

        public static Location GetRandomLocation(
            string newXyz,
            IClientConnection clientConnection,
            MovementDirection enterDirection,
            IClientManager clientManager,
            IWorld world,
            INpcService npcService)
        {
            var (xcoord, ycoord, zcoord) = ParseXyz(newXyz);

            // TODO: Replace with random stuff.

            var newLocation = new Location()
            {
                XCoord = xcoord,
                YCoord = ycoord,
                ZCoord = zcoord,
                Title = "An Empty Place",
                Description = "This empty room will be replaced with some randomly-generated fun later on.",
                IsTemporary = true,
                LastAccessed = DateTimeOffset.Now,
                Exits = new ConcurrentList<MovementDirection>()
                    {
                        MovementDirection.North,
                        MovementDirection.East,
                        MovementDirection.South,
                        MovementDirection.West
                    }
            };

            npcService.SpawnNpcsForParty(clientConnection, newLocation);

            return newLocation;
        }

        public static (long xcoord, long ycoord, string zcoord) ParseXyz(string xyz)
        {
            var split = xyz.Split(",");
            var x = split[0].ToString().Trim();
            var y = split[1].ToString().Trim();
            var z = split[2].ToString().Trim();
            return (long.Parse(x), long.Parse(y), z);
        }

        public void MoveCharacter(IClientConnection clientConnection, MovementDirection direction)
        {
            if (clientConnection.Character.State != CharacterState.Normal)
            {
                return;
            }

            var oldLocation = clientConnection.Location;

            string newXyz;

            switch (direction)
            {
                case MovementDirection.North when oldLocation.Exits.Contains(MovementDirection.North):
                    newXyz = $"{oldLocation.XCoord},{oldLocation.YCoord - 1},{oldLocation.ZCoord}";
                    break;
                case MovementDirection.East when oldLocation.Exits.Contains(MovementDirection.East):
                    newXyz = $"{oldLocation.XCoord + 1},{oldLocation.YCoord},{oldLocation.ZCoord}";
                    break;
                case MovementDirection.South when oldLocation.Exits.Contains(MovementDirection.South):
                    newXyz = $"{oldLocation.XCoord},{oldLocation.YCoord + 1},{oldLocation.ZCoord}";
                    break;
                case MovementDirection.West when oldLocation.Exits.Contains(MovementDirection.West):
                    newXyz = $"{oldLocation.XCoord - 1},{oldLocation.YCoord},{oldLocation.ZCoord}";
                    break;
                default:
                    clientConnection.InvokeMessageReceived(new LocalEventMessage("There is no exit in that direction."));
                    return;
            }

            
            if (!_world.Locations.TryGet(newXyz, out var newLocation))
            {
                newLocation = GetRandomLocation(newXyz, clientConnection, direction, _clientManager, _world, _npcService);
                _world.Locations.AddOrUpdate(newLocation.XYZ, newLocation);
            }
            else
            {
                AddLogicalExits(newLocation.Exits, _world, newXyz);
            }

            oldLocation.LastAccessed = DateTimeOffset.Now;
            newLocation.LastAccessed = DateTimeOffset.Now;
            oldLocation.RemoveCharacter(clientConnection.Character);
            newLocation.AddCharacter(clientConnection.Character);
            clientConnection.Location = newLocation;
            clientConnection.Character.FarthestDistanceTravelled = (long)Calculator.GetDistanceBetween(0, 0, newLocation.XCoord, newLocation.YCoord);
            clientConnection.Character.GuardAmount = 0;

            _clientManager.SendToOtherLocals(clientConnection, oldLocation, 
                new LocalEventMessage($"{clientConnection.Character.Name} left to the {direction}."));

            _clientManager.SendToOtherLocals(clientConnection, newLocation,
                new LocalEventMessage($"{clientConnection.Character.Name} entered from the {GetOppositeDirection(direction)}."));
            clientConnection.InvokeMessageReceived(GenericMessage.LocationChanged);

            _combatService.InitiateNpcAttackOnSight(clientConnection);
        }
    }
}
