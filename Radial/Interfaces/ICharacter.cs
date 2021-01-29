using Radial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Interfaces
{
    public interface ICharacter
    {
        long CoreEnergy { get; }
        long CurrentEnergy { get; }

        List<CharacterEffect> Effects { get; }
    }
}
