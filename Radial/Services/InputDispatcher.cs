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
        Task Attack(IClientConnection clientConnection);

        Task Blast(IClientConnection clientConnection);

        Task Block(IClientConnection clientConnection);

        Task EscapeCombat(IClientConnection clientConnection);
        Task Heal(IClientConnection clientConnection);

        Task MoveCharacter(IClientConnection clientConnection, MovementDirection direction);
        Task Respawn(IClientConnection clientConnection);
    }

    public class InputDispatcher : IInputDispatcher
    {
        public async Task Attack(IClientConnection clientConnection)
        {

        }

        public async Task Blast(IClientConnection clientConnection)
        {

        }

        public async Task Block(IClientConnection clientConnection)
        {

        }

        public async Task EscapeCombat(IClientConnection clientConnection)
        {

        }

        public async Task Heal(IClientConnection clientConnection)
        {

        }

        public async Task MoveCharacter(IClientConnection clientConnection, MovementDirection direction)
        {

        }

        public async Task Respawn(IClientConnection clientConnection)
        {

        }
    }
}
