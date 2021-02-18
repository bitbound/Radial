using Radial.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Radial.Data.Entities
{
    public class CharacterEffect
    {

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }

        public TimeSpan Duration { get; init; }
        public DateTimeOffset StartTime { get; init; }
        public long StatChange { get; init; }
        public CharacterEffectStat TargetStat { get; init; }
        public EffectType Type { get; init; }
    }
}
