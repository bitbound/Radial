using Microsoft.AspNetCore.Identity;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser
    {
        public CharacterInfo Character { get; init; }
        public bool IsServerAdmin { get; set; }
    }
}
