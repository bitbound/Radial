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
        double GetDistanceBetween(long fromX, long fromY, long toX, long toY);
    }

    public class LocationService : ILocationService
    {
        private readonly IWorld _world;
        private readonly IClientManager _clientManager;

        public LocationService(IWorld world, IClientManager clientManager)
        {
            _world = world;
            _clientManager = clientManager;
        }

        public double GetDistanceBetween(long fromX, long fromY, long toX, long toY)
        {
            return Math.Sqrt(Math.Pow(fromX - toX, 2) +
                Math.Pow(fromY - toY, 2));
        }

        public void MoveCharacter(IClientConnection clientConnection, MovementDirection direction)
        {
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
                    clientConnection.InvokeMessageReceived(new LocalEventMessage()
                    {
                        Message = "There is no exit in that direction."
                    });
                    return;
            }

            
            if (!_world.Locations.TryGet(newXyz, out var newLocation))
            {
                newLocation = GetRandomLocation(newXyz, clientConnection);
            }
            oldLocation.Characters.Remove(clientConnection.Character);
            newLocation.Characters.Add(clientConnection.Character);
        }

        private Location GetRandomLocation(string newXyz, IClientConnection clientConnection)
        {
            var (xcoord, ycoord, zcoord) = ParseXyz(newXyz);
            var distanceFromCenter = GetDistanceBetween(0, 0, xcoord, ycoord);
            var partyMembers = _clientManager.GetPartyMembers(clientConnection);

            var npcsToSpawn = new Random().Next(0, partyMembers.Count() + 1);

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
                Characters = new ConcurrentList<CharacterBase>()
            };

            for (var i = 0; i < npcsToSpawn; i++)
            {
                newLocation.Characters.Add(new Npc()
                {
                    Name = $"A Wandering Entity ({i})",
                    AggressionModel = AggressionModel.PlayerOnSight,
                    CoreEnergy = new Random(i).Next((int)distanceFromCenter / 2, (int)distanceFromCenter),
                    Type = CharacterType.NPC
                });;
            }

            return newLocation;
        }

        private (long xcoord, long ycoord, string zcoord) ParseXyz(string xyz)
        {
            var split = xyz.Split(",");
            var x = split[0].ToString().Trim();
            var y = split[1].ToString().Trim();
            var z = split[2].ToString().Trim();
            return (long.Parse(x), long.Parse(y), z);
        }
    }
}
