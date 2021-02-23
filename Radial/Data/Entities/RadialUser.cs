using Microsoft.AspNetCore.Identity;
using Radial.Models;
using System;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser
    {
        public bool IsServerAdmin { get; set; }
    }
}
