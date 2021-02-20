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

        public CharacterInfo()
        {
            EnergyCurrent = EnergyMax;
        }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }

        [NotMapped]
        public int ActionBonus { get; set; }

        public Location Location { get; set; }

        [NotMapped]
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
            .Where(x => x.TargetStat == CharacterEffectStat.EnergyMax)
            .Sum(x => x.StatChange);

        [NotMapped]
        public double EnergyPercent => (double)EnergyCurrent / EnergyMax;

        [NotMapped]
        public DateTimeOffset LastMoveTime { get; set; }

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
