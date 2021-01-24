using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class PlayerInfo
    {
        public string Username { get; set; }

        public string XYZ => $"{XCoord},{YCoord},{ZCoord}";
        public long XCoord { get; set; }
        public long YCoord { get; set; }
        public string ZCoord { get; set; }

        public string PartyId { get; set; }
    }
}
