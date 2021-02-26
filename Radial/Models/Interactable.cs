using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class Interactable
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Content { get; init; }

        public string Prompt { get; set; }

        public string Title { get; init; }

        public ConcurrentDictionary<Guid, Interactable> Interactables { get; init; } = new ConcurrentDictionary<Guid, Interactable>();
    }
}
