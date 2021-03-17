using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Radial.Models
{
    public class PlayerCharacter : CharacterBase
    {
        public PlayerSettings Settings { get; set; } = new();
        public PlayerStats Stats { get; set; } = new();

        [JsonIgnore]
        public Interactable CurrentInteraction { get; set; }

        /// <summary>
        /// A lookup for which interactable to start at when interacting with an NPC or object.
        /// The key is the NPC or object ID, the value is the interable ID at which to start.
        /// This is so state can be saved, like for NPC dialogs.
        /// </summary>
        public ConcurrentDictionary<Guid, Guid> InteractableStartLookup { get; init; } = new();

        [JsonIgnore]
        public DateTimeOffset LastCombatEncounter { get; set; }

        [JsonIgnore]
        public Party Party { get; set; }

        [JsonIgnore]
        public List<Party> PartyInvites { get; init; } = new();

        public string UserId { get; set; }
    }
}
