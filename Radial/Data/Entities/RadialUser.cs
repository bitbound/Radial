using Microsoft.AspNetCore.Identity;
using Radial.Models;
using System;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser
    {
        public bool IsBanned { get; set; }
        public bool IsServerAdmin { get; set; }
    }
}
