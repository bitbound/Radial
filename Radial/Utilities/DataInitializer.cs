using Radial.Data.Entities;
using Radial.Models;
using Radial.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Utilities
{
    public static class DataInitializer
    {
        public static void Load(IWorld world)
        {
            foreach (var location in Locations)
            {
                world.Locations.AddOrUpdate(location.XYZ, location);
            }
        }

        private static Location[] Locations => new [] 
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
