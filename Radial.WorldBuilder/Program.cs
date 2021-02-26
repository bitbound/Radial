using Radial.Enums;
using Radial.Models;
using Radial.Services;
using Radial.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Radial.WorldBuilder
{
    class Program
    {
        static void Main(string[] args)
        {
            var locationDictionary = new ConcurrentDictionary<string, Location>();
            foreach (var location in Locations)
            {
                locationDictionary.TryAdd(location.XYZ, location);
            }

            File.WriteAllText("Locations.json", JsonSerializer.Serialize(locationDictionary));
        }

        private static Location[] Locations => new[]
        {
            new Location()
            {
                XCoord = 0,
                YCoord = 0,
                ZCoord = World.PurgatoryZCoord,
                Title = "Purgatory",
                Description = "There is only absence here.  The void is like a tangible force that you can feel, " +
                    "so powerful that the concept of your own existence becomes a confusing blur, unraveling in your own mind.",
                IsSafeArea = true
            },
            new Location()
            {
                XCoord = 0,
                YCoord = 0,
                ZCoord = World.OfflineZCoord,
                Title = "Offline",
                Description = "You're offline!  You shouldn't be seeing this!  Email the dev immediately!",
                IsSafeArea = true
            },
            new Location()
            {
               XCoord = 0,
               YCoord = 0,
               ZCoord = "0",
               Title = "The Center",
               Description = "This is the center of the infinite nothingness.",
               IsSafeArea = true,
               Exits = new ConcurrentList<MovementDirection>()
               {
                   MovementDirection.North,
                   MovementDirection.East,
                   MovementDirection.South,
                   MovementDirection.West
               }
            }
        };
    }
}
