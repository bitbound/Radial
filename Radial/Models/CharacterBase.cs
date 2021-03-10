using Radial.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class CharacterBase
    {
        private static readonly long _lowestStatValue = 10;
        private CharacterState _state;

        public long AttributeAttack { get; set; }
        public long AttributeBlast { get; set; }

        public long AttributeGuard { get; set; }

        public long AttributeHeal { get; set; }


        [JsonIgnore]
        public long AttributePointsRemaining =>
            CorePower - AttributeAttack - AttributeBlast - AttributeGuard - AttributeHeal;

        [JsonIgnore]
        public long ChargeCurrent { get; set; }

        [JsonIgnore]
        public long ChargeMax => Math.Max(_lowestStatValue, CorePowerCurrent + ChargeMaxMod);

        [JsonIgnore]
        public long ChargeMaxMod => Effects
            .Where(x => x.TargetStat == CharacterEffectStat.ChargeMax)
            .Sum(x => x.StatChange);

        [JsonIgnore]
        public double ChargePercent => (double)ChargeCurrent / ChargeMax;

        [JsonIgnore]
        public string ChargePercentFormatted => $"{Math.Round(ChargePercent * 100)}%";

        [JsonIgnore]
        public long ChargeRate => Math.Max(_lowestStatValue, CorePowerCurrent + ChargeRateMod);

        [JsonIgnore]
        public long ChargeRateMod => Effects
            .Where(x => x.TargetStat == CharacterEffectStat.ChargeRate)
            .Sum(x => x.StatChange);


        public long CorePower { get; set; }

        [JsonIgnore]
        public long CorePowerCurrent => Math.Max(_lowestStatValue, CorePower + CorePowerMod);

        [JsonIgnore]
        public long CorePowerMod => Effects
            .Where(x => x.TargetStat == CharacterEffectStat.CoreEnergy)
            .Sum(x => x.StatChange);

        public List<CharacterEffect> Effects { get; init; } = new List<CharacterEffect>();

        public long EnergyCurrent { get; set; }

        [JsonIgnore]
        public long EnergyMax => CorePowerCurrent + EnergyMaxMod;

        [JsonIgnore]
        public long EnergyMaxMod => Effects
            .Where(x => x.TargetStat == CharacterEffectStat.EnergyMax)
            .Sum(x => x.StatChange);

        [JsonIgnore]
        public double EnergyPercent => (double)EnergyCurrent / EnergyMax;

        [JsonIgnore]
        public string EnergyPercentFormatted => $"{Math.Round(EnergyPercent * 100)}%";

        public long Experience { get; set; }

        [JsonIgnore]
        public long ExperienceToLevel => CorePower - Experience;

        [JsonIgnore]
        public double ExperiencePercent => (double)Experience / (double)CorePower;

        [JsonIgnore]
        public string ExperiencePercentFormatted => $"{Math.Round(ExperiencePercent * 100)}%";

        public long Glint { get; set; }

        [JsonIgnore]
        public Interactable Interaction { get; set; }

        [JsonIgnore]
        public bool IsGuarding { get; set; }
        [JsonIgnore]
        public DateTimeOffset LastMoveTime { get; set; }

        public string Name { get; init; }


        [JsonIgnore]
        public CharacterState State
        {
            get
            {
                if (EnergyCurrent < 1)
                {
                    return CharacterState.Dead;
                }
                return _state;
            }
            set
            {
                _state = value;
            }
        }

        [JsonIgnore]
        public CharacterBase Target { get; set; }

        public CharacterType Type { get; set; }
    }
}
