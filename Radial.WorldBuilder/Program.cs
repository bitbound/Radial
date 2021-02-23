using Radial.Models;
using Radial.Services;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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
            File.WriteAllTextAsync("Locations.json", JsonSerializer.Serialize(locationDictionary));
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
            },
            new Location()
            {
                XCoord = 0,
                YCoord = 0,
                ZCoord = World.OfflineZCoord,
                Title = "Offline",
                Description = "You're offline!  You shouldn't be seeing this!  Email the dev immediately!",
            },
            new Location()
            {
               XCoord = 0,
               YCoord = 0,
               ZCoord = "0",
               Title = "The Center",
               Description = "This is the center of the infinite nothingness.",
            }
        };
    }
}
