using Radial.Data.Entities;
using Radial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Interfaces
{
    public interface ICharacter
    {
        long ChargeCurrent { get; }
        long ChargeMax { get; }
        long ChargeMaxMod { get; }

        long CoreEnergy { get; }

        List<CharacterEffect> Effects { get; }

        long EnergyCurrent { get; }
        long EnergyMax { get; }
        long EnergyMaxMod { get; }

        long XCoord { get; }
        string XYZ { get; }
        long YCoord { get; }
        string ZCoord { get; }
    }
}
