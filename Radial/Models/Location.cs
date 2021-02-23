using Radial.Models;
using System;
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
        public List<Character> Characters { get; set; } = new List<Character>();

        public string Description { get; set; }

        public List<Interactable> Interactables { get; set; }
        public bool IsEditable { get; set; }

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
