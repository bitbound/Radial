using Radial.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Radial.Data.Entities
{
    public class Location
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public List<Character> Characters { get; set; } = new List<Character>();

        public string Description { get; set; }

        public List<Interactable> Interactables { get; set; }
        public bool IsEditable { get; set; }

        [JsonIgnore]
        public List<Npc> Npcs => Characters.OfType<Npc>().ToList();

        [JsonIgnore]
        public List<PlayerCharacter> Players => Characters.OfType<PlayerCharacter>().ToList();

        public string Title { get; set; }
        public long XCoord { get; init; }

        [JsonIgnore]
        public string XYZ => $"{XCoord},{YCoord},{ZCoord}";
        public long YCoord { get; init; }
        public string ZCoord { get; init; } = "0";
    }
}
