using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Data.Entities
{
    public class PlayerCharacter : CharacterInfo
    {
        public string UserId { get; set; }
        public virtual RadialUser User { get; set; }
    }
}
