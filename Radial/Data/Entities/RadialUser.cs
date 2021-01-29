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
        public long CoreEnergy { get; set; }
        public long CurrentEnergy { get; set; }

        public List<CharacterEffect> Effects { get; set; } = new List<CharacterEffect>();

        [NotMapped]
        public long EnergyMax
        {
            get
            {
                return CoreEnergy + EnergyMod;
            }
        }

        [NotMapped]
        public long EnergyMod => Effects
            .Where(x => x.TargetStat == Enums.CharacterEffectStat.MaxEnergy)
            .Sum(x => x.StatChange);

        public long XCoord { get; set; }
        public long YCoord { get; set; }
        public string ZCoord { get; set; }

        [NotMapped]
        public string XYZ => $"{XCoord},{YCoord},{ZCoord}";

        [NotMapped]
        public string PartyId { get; set; }
    }
}
