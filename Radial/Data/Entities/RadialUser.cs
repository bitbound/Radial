using Microsoft.AspNetCore.Identity;
using System;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser
    {
        public Guid CharacterId { get; set; }
        public bool IsServerAdmin { get; set; }
    }
}
