using Radial.Enums;
using Radial.Models;
using Radial.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class Location
    {
        public ConcurrentList<CharacterBase> Characters { get; set; } = new ConcurrentList<CharacterBase>();

        public string Description { get; set; }

        public ConcurrentList<MovementDirection> Exits { get; set; } = new ConcurrentList<MovementDirection>();

        public ConcurrentList<Interactable> Interactables { get; set; } = new ConcurrentList<Interactable>();
        public bool IsEditable { get; set; }
        public bool IsSafeArea { get; set; }
        public bool IsTemporary { get; set; }

        public DateTimeOffset LastAccessed { get; set; }

        [JsonIgnore]
        public IEnumerable<Npc> Npcs => Characters.OfType<Npc>();

        [JsonIgnore]
        public IEnumerable<PlayerCharacter> Players => Characters.OfType<PlayerCharacter>();

        public string Title { get; set; }
        public long XCoord { get; init; }

        [JsonIgnore]
        public string XYZ => $"{XCoord},{YCoord},{ZCoord}";
        public long YCoord { get; init; }
        public string ZCoord { get; init; } = "0";
    }
}
