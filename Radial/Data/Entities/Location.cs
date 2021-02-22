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


        public virtual List<CharacterInfo> Characters { get; set; } = new List<CharacterInfo>();

        public string Description { get; set; }

        public virtual List<Interactable> Interactables { get; set; }
        public bool IsEditable { get; set; }

        [NotMapped]
        public List<Npc> Npcs => Characters.OfType<Npc>().ToList();

        [NotMapped]
        public List<PlayerCharacter> Players => Characters.OfType<PlayerCharacter>().ToList();

        public string Title { get; set; }
        public long XCoord { get; init; }

        [NotMapped]
        public string XYZ => $"{XCoord},{YCoord},{ZCoord}";
        public long YCoord { get; init; }
        public string ZCoord { get; init; } = "0";
    }
}
