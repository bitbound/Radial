using Radial.Enums;
using Radial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Utilities
{
    public static class Calculator
    {
        public static Random RandInstance { get; } = new Random();


        public static double GetDistanceBetween(long fromX, long fromY, long toX, long toY)
        {
            return Math.Sqrt(Math.Pow(fromX - toX, 2) +
                Math.Pow(fromY - toY, 2));
        }

        public static bool RollForBool(double odds)
        {
            if (odds < 0 || odds > 1)
            {
                throw new ArgumentException("Odds must be between 0 and 1.", nameof(odds));
            }

            return RandInstance.NextDouble() <= odds;
        }

        public static T GetRandomEnum<T>() 
            where T : Enum
        {
            var values = Enum.GetValues(typeof(T)).Cast<T>().ToArray();
            var randomIndex = RandInstance.Next(0, values.Length);
            return values[randomIndex];
        }

        public static T GetRandom<T>(IEnumerable<T> collection)
        {
            if (collection?.Any() != true)
            {
                return default;
            }

            var count = collection.Count();
            var index = RandInstance.Next(0, count);
            return collection.ElementAt(index);
        }
    }
}
