using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Interfaces
{
    public interface IInteraction
    {
        Guid Id { get; }

        string DisplayText { get; }

        IList<IInteraction> InteractionLinks { get; }
    }
}
