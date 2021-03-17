using Radial.Enums;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Radial.Models
{
    public class Location
    {
        [JsonIgnore]
        public IEnumerable<CharacterBase> Characters => Npcs.Cast<CharacterBase>().Concat(Players.Cast<CharacterBase>());

        [JsonIgnore]
        public IEnumerable<CharacterBase> CharactersAlive => Characters.Where(x => x.State != CharacterState.Dead);

        public string Description { get; set; }

        public ConcurrentList<MovementDirection> Exits { get; set; } = new();

         public Guid[] Interactables { get; init; } = Array.Empty<Guid>();

        public bool IsEditable { get; set; }
        public bool IsSafeArea { get; set; }
        public bool IsTemporary { get; set; }

        public DateTimeOffset LastAccessed { get; set; }

        public ConcurrentList<Npc> Npcs { get; init; } = new();

        [JsonIgnore]
        public IEnumerable<Npc> NpcsAlive => Npcs.Where(x => x.State != CharacterState.Dead);

        public ConcurrentList<PlayerCharacter> Players { get; init; } = new();

        [JsonIgnore]
        public IEnumerable<PlayerCharacter> PlayersAlive => Players.Where(x => x.State != CharacterState.Dead);

        public string Title { get; set; }
        public long XCoord { get; init; }

        [JsonIgnore]
        public string XYZ => $"{XCoord},{YCoord},{ZCoord}";
        public long YCoord { get; init; }
        public string ZCoord { get; init; } = "0";

        public void AddCharacter(CharacterBase character)
        {
            if (character.Type == CharacterType.NPC)
            {
                Npcs.Add((Npc)character);
            }
            else if (character.Type == CharacterType.Player)
            {
                Players.Add((PlayerCharacter)character);
            }
        }

        public void RemoveCharacter(CharacterBase character)
        {
            if (character.Type == CharacterType.NPC)
            {
                Npcs.Remove((Npc)character);
            }
            else if (character.Type == CharacterType.Player)
            {
                Players.Remove((PlayerCharacter)character);
            }
        }
    }
}
