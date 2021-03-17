using Radial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IInteractableService
    {
        bool GetInteractable(Guid id, out Interactable result);
    }

    public class InteractableService : IInteractableService
    {
        private readonly IWorld _world;

        public InteractableService(IWorld world)
        {
            _world = world;
        }
        public bool GetInteractable(Guid id, out Interactable result)
        {
            return _world.Interactables.TryGet(id.ToString(), out result);
        }
    }
}
