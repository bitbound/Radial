using Radial.Data.Entities;
using Radial.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class Npc : CharacterBase
    {
        public Guid Id { get; init; } = Guid.NewGuid();
        public AggressionModel AggressionModel { get; init; }
        public ConcurrentDictionary<Guid, Interactable> Dialog { get; init; } = new();
        public bool IsFriendly { get; set; }
        public bool IsRespawnable { get; set; }
        public string OnPlayerSightedScript { get; init; }
    }
}
