using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Radial.Models.Dtos
{
    public interface IBaseDto
    {
        public DtoType DtoType { get; }
    }
}
