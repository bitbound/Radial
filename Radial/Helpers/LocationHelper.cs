﻿using Radial.Enums;
using Radial.Models;
using Radial.Services;
using Radial.Services.Client;
using Radial.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Helpers
{
    public static class LocationHelper
    {
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

        public static double GetDistanceBetween(long fromX, long fromY, long toX, long toY)
        {
            return Math.Sqrt(Math.Pow(fromX - toX, 2) +
                Math.Pow(fromY - toY, 2));
        }


        // TODO: Decide whether to add randomness to exits on world map.
        public static ConcurrentList<MovementDirection> GetOpenWorldExits(IWorld world, string newXyz, MovementDirection enterDirection)
        {
            var exits = new ConcurrentList<MovementDirection>();
            var random = new Random();

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
                    4 :
                    2;

                var addExit = Convert.ToBoolean(random.Next(0, odds));
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
            IWorld world)
        {
            var (xcoord, ycoord, zcoord) = ParseXyz(newXyz);
            var distanceFromCenter = GetDistanceBetween(0, 0, xcoord, ycoord);
            var partyMembers = clientManager.GetPartyMembers(clientConnection);

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
                Characters = new ConcurrentList<CharacterBase>(),
                Exits = new ConcurrentList<MovementDirection>()
                    {
                        MovementDirection.North,
                        MovementDirection.East,
                        MovementDirection.South,
                        MovementDirection.West
                    }
            };

            for (var i = 0; i < npcsToSpawn; i++)
            {
                newLocation.Characters.Add(new Npc()
                {
                    Name = $"A Wandering Entity",
                    AggressionModel = AggressionModel.PlayerOnSight,
                    CoreEnergy = new Random(i).Next((int)distanceFromCenter / 2, (int)distanceFromCenter),
                    Type = CharacterType.NPC
                });
            }

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
    }
}