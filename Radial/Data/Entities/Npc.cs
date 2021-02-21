using Radial.Data.Entities;
using Radial.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Models
{
    public class Npc : CharacterInfo
    {
        public AggressionModel AggressionModel { get; init; }
        public virtual Interactable Dialog { get; init; }
    }
}
