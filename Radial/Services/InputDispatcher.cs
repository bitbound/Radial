using Radial.Enums;
using Radial.Services.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface IInputDispatcher
    {
        Task MoveCharacter(IClientConnection clientConnection, MovementDirection direction);
    }

    public class InputDispatcher : IInputDispatcher
    {
        public async Task MoveCharacter(IClientConnection clientConnection, MovementDirection direction)
        {

        }
    }
}
