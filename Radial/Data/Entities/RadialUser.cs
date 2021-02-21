using Microsoft.AspNetCore.Identity;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser
    {
        public virtual PlayerCharacter Character { get; init; }
        public bool IsServerAdmin { get; set; }
    }
}
