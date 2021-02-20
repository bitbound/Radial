using Radial.Data.Entities;
using Radial.Enums;
using Radial.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class Npc : ICharacter
    {
        public CharacterInfo Info { get; init; }
        public AggressionModel AggressionModel { get; init; }
    }
}
