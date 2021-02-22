using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Data.Entities
{
    public class Interactable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; init; }

        public string Content { get; init; }

        public string Prompt { get; set; }

        public string Title { get; init; }

        public virtual List<Interactable> Interactables { get; init; } = new List<Interactable>();
    }
}
