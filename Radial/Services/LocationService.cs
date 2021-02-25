using Radial.Enums;
using Radial.Helpers;
using Radial.Models;
using Radial.Models.Messaging;
using Radial.Services.Client;
using Radial.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
    public interface ILocationService
    {
        void MoveCharacter(IClientConnection clientConnection, MovementDirection direction);
    }

    public class LocationService : ILocationService
    {
        private readonly IWorld _world;
        private readonly IClientManager _clientManager;

        public LocationService(IWorld world, IClientManager clientManager)
        {
            _world = world;
            _clientManager = clientManager;
        }

       
        public void MoveCharacter(IClientConnection clientConnection, MovementDirection direction)
        {
            var oldLocation = clientConnection.Location;

            string newXyz;

            switch (direction)
            {
                case MovementDirection.North when oldLocation.Exits.Contains(MovementDirection.North):
                    newXyz = $"{oldLocation.XCoord},{oldLocation.YCoord - 1},{oldLocation.ZCoord}";
                    break;
                case MovementDirection.East when oldLocation.Exits.Contains(MovementDirection.East):
                    newXyz = $"{oldLocation.XCoord + 1},{oldLocation.YCoord},{oldLocation.ZCoord}";
                    break;
                case MovementDirection.South when oldLocation.Exits.Contains(MovementDirection.South):
                    newXyz = $"{oldLocation.XCoord},{oldLocation.YCoord + 1},{oldLocation.ZCoord}";
                    break;
                case MovementDirection.West when oldLocation.Exits.Contains(MovementDirection.West):
                    newXyz = $"{oldLocation.XCoord - 1},{oldLocation.YCoord},{oldLocation.ZCoord}";
                    break;
                default:
                    clientConnection.InvokeMessageReceived(new LocalEventMessage()
                    {
                        Message = "There is no exit in that direction."
                    });
                    return;
            }

            
            if (!_world.Locations.TryGet(newXyz, out var newLocation))
            {
                newLocation = LocationHelper.GetRandomLocation(newXyz, clientConnection, direction, _clientManager, _world);
                _world.Locations.AddOrUpdate(newLocation.XYZ, newLocation);
            }
            else
            {
                LocationHelper.AddLogicalExits(newLocation.Exits, _world, newXyz);
            }
        
            oldLocation.Characters.Remove(clientConnection.Character);
            newLocation.Characters.Add(clientConnection.Character);
            clientConnection.Location = newLocation;
            _clientManager.SendToLocals(clientConnection, oldLocation, new LocalEventMessage()
            {
                Message = $"{clientConnection.Character.Name} left to the {direction}."
            });
            _clientManager.SendToLocals(clientConnection, newLocation, new LocalEventMessage()
            {
                Message = $"{clientConnection.Character.Name} entered from the {LocationHelper.GetOppositeDirection(direction)}."
            });
            clientConnection.InvokeMessageReceived(GenericMessage.LocationChanged);
        }
    }
}
