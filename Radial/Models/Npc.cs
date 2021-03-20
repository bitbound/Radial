using Radial.Enums;
using System;
using System.Collections.Concurrent;

namespace Radial.Models
{
    public class Npc : CharacterBase
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public AggressionModel AggressionModel { get; init; }
        public Guid? Dialog { get; init; }
        public bool IsFriendly { get; set; }
        public bool IsRespawnable { get; set; }
        public string OnPlayerSightedScript { get; init; }
    }
}
