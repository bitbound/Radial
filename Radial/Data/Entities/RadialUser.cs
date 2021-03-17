using Microsoft.AspNetCore.Identity;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser
    {
        public bool IsBanned { get; set; }
        public bool IsServerAdmin { get; set; }
    }
}
