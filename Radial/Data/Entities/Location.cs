using Radial.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Data.Entities
{
    public class Location
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }


        [NotMapped]
        public List<CharacterInfo> Characters =>
            Players
                .Select(x => x.Character)
                .Concat(Npcs.Select(x => x.Character))
                .ToList();

        public string Description { get; init; }

        public List<Interactable> Interactables { get; set; }
        public bool IsEditable { get; set; }
        public List<Npc> Npcs { get; init; } = new List<Npc>();
        public List<RadialUser> Players { get; init; } = new List<RadialUser>();
        public string Title { get; init; }
        public long XCoord { get; init; }

        [NotMapped]
        public string XYZ => $"{XCoord},{YCoord},{ZCoord}";
        public long YCoord { get; init; }
        public string ZCoord { get; init; } = "0";
    }
}
