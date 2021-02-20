using Microsoft.AspNetCore.Identity;
using Radial.Interfaces;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser, ICharacter
    {
        public CharacterInfo Info { get; init; }
        public bool IsServerAdmin { get; set; }
    }
}
