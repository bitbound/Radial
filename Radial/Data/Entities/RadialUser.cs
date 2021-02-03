using Microsoft.AspNetCore.Identity;
using Radial.Interfaces;
using Radial.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser, ICharacter
    {
        private static readonly long _lowestStatValue = 10;
        public RadialUser()
        {
            EnergyCurrent = EnergyMax;
        }

        public long ChargeCurrent { get; set; }
        [NotMapped]
        public long ChargeMax => Math.Max(_lowestStatValue, CoreEnergy + ChargeMaxMod);
        [NotMapped]
        public long ChargeMaxMod => Effects
            .Where(x => x.TargetStat == Enums.CharacterEffectStat.ChargeMax)
            .Sum(x => x.StatChange);

        [NotMapped]
        public double ChargePercent => (double)ChargeCurrent / ChargeMax;

        [NotMapped]
        public long ChargeRate => Math.Max(_lowestStatValue, CoreEnergy + ChargeRateMod);
        [NotMapped]
        public long ChargeRateMod => Effects
            .Where(x => x.TargetStat == Enums.CharacterEffectStat.ChargeRate)
            .Sum(x => x.StatChange);

        public long CoreEnergy { get; set; }

        public List<CharacterEffect> Effects { get; set; } = new List<CharacterEffect>();

        public long EnergyCurrent { get; set; }

        [NotMapped]
        public long EnergyMax => CoreEnergy + EnergyMaxMod;

        [NotMapped]
        public long EnergyMaxMod => Effects
            .Where(x => x.TargetStat == Enums.CharacterEffectStat.EnergyMax)
            .Sum(x => x.StatChange);

        [NotMapped]
        public double EnergyPercent => (double)EnergyCurrent / EnergyMax;

        public bool IsServerAdmin { get; set; }

        [NotMapped]
        public string PartyId { get; set; }

        public long XCoord { get; set; }
        [NotMapped]
        public string XYZ => $"{XCoord},{YCoord},{ZCoord}";

        public long YCoord { get; set; }
        public string ZCoord { get; set; }
    }
}
