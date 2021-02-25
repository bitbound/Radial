using System;
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
    }
}
