using System;
using System.Collections.Generic;

namespace Radial.Models
{
    public class Party
    {
        public Party(PlayerCharacter leader)
        {
            Leader = leader;
        }
        public Guid Id { get; init; } = Guid.NewGuid();
        public List<PlayerCharacter> Members { get; init; } = new();

        public PlayerCharacter Leader { get; set; }
    }
}
