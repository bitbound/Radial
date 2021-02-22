using Radial.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Data.Entities
{
    public class CharacterInfo
    {
        private static readonly long _lowestStatValue = 10;

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [NotMapped]
        public double ActionBonus { get; set; }


        [NotMapped]
        public long ChargeCurrent { get; set; }

        [NotMapped]
        public long ChargeMax => Math.Max(_lowestStatValue, CoreEnergyCurrent + ChargeMaxMod);

        [NotMapped]
        public long ChargeMaxMod => Effects
            .Where(x => x.TargetStat == CharacterEffectStat.ChargeMax)
            .Sum(x => x.StatChange);

        [NotMapped]
        public double ChargePercent => (double)ChargeCurrent / ChargeMax;

        [NotMapped]
        public long ChargeRate => Math.Max(_lowestStatValue, CoreEnergyCurrent + ChargeRateMod);

        [NotMapped]
        public long ChargeRateMod => Effects
            .Where(x => x.TargetStat == CharacterEffectStat.ChargeRate)
            .Sum(x => x.StatChange);


        public long CoreEnergy { get; set; }

        [NotMapped]
        public long CoreEnergyCurrent => Math.Max(_lowestStatValue, CoreEnergy + CoreEnergyMod);

        [NotMapped]
        public long CoreEnergyMod => Effects
            .Where(x => x.TargetStat == CharacterEffectStat.CoreEnergy)
            .Sum(x => x.StatChange);

        public virtual List<CharacterEffect> Effects { get; set; } = new List<CharacterEffect>();

        public long EnergyCurrent { get; set; }

        [NotMapped]
        public long EnergyMax => CoreEnergyCurrent + EnergyMaxMod;

        [NotMapped]
        public long EnergyMaxMod => Effects
            .Where(x => x.TargetStat == CharacterEffectStat.EnergyMax)
            .Sum(x => x.StatChange);

        [NotMapped]
        public double EnergyPercent => (double)EnergyCurrent / EnergyMax;

        public long Experience { get; set; }

        [NotMapped]
        public Interactable Interaction { get; set; }

        [NotMapped]
        public DateTimeOffset LastMoveTime { get; set; }

        public virtual Location Location { get; set; }
        public Guid LocationId { get; set; }

        public string Name { get; init; }

        [NotMapped]
        public string PartyId { get; set; }

        [NotMapped]
        public CharacterState State { get; set; }

        [NotMapped]
        public CharacterInfo Target { get; set; }

        public CharacterType Type { get; set; }


        [NotMapped]
        public long XCoord => Location?.XCoord ?? 0;
        [NotMapped]
        public string XYZ => Location?.XYZ ?? "0,0,0";
        [NotMapped]
        public long YCoord => Location?.YCoord ?? 0;
        [NotMapped]
        public string ZCoord => Location?.ZCoord ?? "0";
    }
}
