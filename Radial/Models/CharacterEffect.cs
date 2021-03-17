using Radial.Enums;
using System;

namespace Radial.Models
{
    public class CharacterEffect
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public TimeSpan Duration { get; init; }
        public DateTimeOffset StartTime { get; init; }
        public long StatChange { get; init; }
        public CharacterEffectStat TargetStat { get; init; }
        public EffectType Type { get; init; }
    }
}
