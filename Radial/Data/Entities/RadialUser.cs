using Microsoft.AspNetCore.Identity;
using Radial.Interfaces;
using Radial.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Data.Entities
{
    public class RadialUser : IdentityUser, ICharacter
    {
        public CharacterInfo Info { get; init; }   
    }
}
