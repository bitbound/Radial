using Radial.Data.Entities;
using Radial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Interfaces
{
    public interface ICharacter
    {
        public CharacterInfo Info { get; }
    }
}
