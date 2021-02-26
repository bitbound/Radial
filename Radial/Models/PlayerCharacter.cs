using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class PlayerCharacter : CharacterBase
    {
        public string UserId { get; set; }

        [JsonIgnore]
        public DateTimeOffset LastCombatEncounter { get; set; }

        /// <summary>
        /// A lookup for which interactable to start at when interacting with an NPC or object.
        /// The key is the NPC or object ID, the value is the interable ID at which to start.
        /// This is so state can be saved, like for NPC dialogs.
        /// </summary>
        public ConcurrentDictionary<Guid, Guid> InteractableStartLookup { get; init; } = new ConcurrentDictionary<Guid, Guid>();
    }
}
