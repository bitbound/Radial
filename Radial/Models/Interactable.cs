using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace Radial.Models
{
    public class Interactable
    {
        public Guid Id { get; init; } = Guid.NewGuid();

        public string Content { get; init; }
        public string OnExecutedScript { get; init; }
        public string Prompt { get; set; }
        public string Title { get; init; }
        public Guid[] Interactables { get; init; } = Array.Empty<Guid>();
    }
}
