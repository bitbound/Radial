using Radial.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Utilities
{
    public static class DataInitializer
    {
        public static void LoadRecords(Data.ApplicationDbContext dbContext)
        {
            if (dbContext.Locations.Any())
            {
                return;
            }

            dbContext.Locations.AddRange(Locations);
            dbContext.SaveChanges();
        }

        private static Location[] Locations => new [] 
        { 
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
