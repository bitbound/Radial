using Radial.Models.Messaging;
using Radial.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Radial.Models;
using Microsoft.AspNetCore.Identity;
using Radial.Data.Entities;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Components.Server.Circuits;

namespace Radial.Services.Client
{
    public interface IClientConnection
    {
        event EventHandler<string> Disconnected;

        event EventHandler<IMessageBase> MessageReceived;

        PlayerCharacter Character { get; }
        Location Location { get; set; }
        RadialUser User { get; }
        Task Connect();
        void Disconnect(string reason);

        void InvokeMessageReceived(IMessageBase message);
    }

    public class ClientConnection : CircuitHandler, IClientConnection
    {
        private readonly AuthenticationStateProvider _authProvider;
        private readonly IClientManager _clientManager;
        private readonly IJsInterop _jsInterop;
        private readonly ILogger<ClientConnection> _logger;
        private readonly UserManager<RadialUser> _userManager;
        private readonly IWorld _world;
        private PlayerCharacter _character;
        private Location _location;
        private RadialUser _user;
        public ClientConnection(
            AuthenticationStateProvider authProvider,
            UserManager<RadialUser> userManager,
            IClientManager clientManager,
            IWorld world,
            IJsInterop jsInterop,
            ILogger<ClientConnection> logger)
        {
            _authProvider = authProvider;
            _userManager = userManager;
            _clientManager = clientManager;
            _jsInterop = jsInterop;
            _world = world;
            _logger = logger;
        }

        public event EventHandler<string> Disconnected;

        public event EventHandler<IMessageBase> MessageReceived;
        public PlayerCharacter Character
        {
            get
            {
                var authState = _authProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
                if (!authState.User.Identity.IsAuthenticated)
                {
                    return null;
                }

                if (_character is not null)
                {
                    return _character;
                }

                _character = _world.PlayerCharacters.FirstOrDefault(x => x.Name == authState.User.Identity.Name);

                if (_character is null)
                {
                    if (!_world.CharacterBackups.TryGet(authState.User.Identity.Name, out _character))
                    {
                        _logger.LogError("Failed to load character.  Username: {username}", authState.User.Identity.Name);
                    }
                    _world.PurgatoryLocation.AddCharacter(_character);
                }
                return _character;
            }
        }

        public Location Location
        {
            get
            {
                var authState = _authProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
                if (!authState.User.Identity.IsAuthenticated)
                {
                    return null;
                }

                if (_location?.Players?.Contains(Character) == true)
                {
                    return _location;
                }

                var location = _world.Locations.Find(x => x.Characters.Contains(Character));
                if (location is null)
                {
                    _world.PurgatoryLocation.AddCharacter(Character);
                    location = _world.PurgatoryLocation;
                }

                return location;
            }
            set
            {
                _location = value;
            }
        }

        public RadialUser User
        {
            get
            {
                var authState = _authProvider.GetAuthenticationStateAsync().GetAwaiter().GetResult();
                if (!authState.User.Identity.IsAuthenticated)
                {
                    return null;
                }

                if (_user is null)
                {
                    _user = _userManager.GetUserAsync(authState.User).GetAwaiter().GetResult();
                }
                return _user;
            }
        }

        public async Task Connect()
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();

        }

        public void Disconnect(string reason)
        {
            Disconnected?.Invoke(this, reason);
        }

        public void InvokeMessageReceived(IMessageBase message)
        {
            _ = Task.Run(() => MessageReceived?.Invoke(this, message));
        }

        public override async Task OnCircuitClosedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            Disconnect("Session closed.");
            if (authState.User?.Identity?.IsAuthenticated == true)
            {
                await _clientManager.RemoveClient(this);
            }
            await base.OnCircuitClosedAsync(circuit, cancellationToken);
        }
        public override async Task OnCircuitOpenedAsync(Circuit circuit, CancellationToken cancellationToken)
        {
            var authState = await _authProvider.GetAuthenticationStateAsync();
            if (authState.User?.Identity?.IsAuthenticated == true)
            {
                await _clientManager.AddClient(this);
                _jsInterop.AddBeforeUnloadHandler();
            }
            await base.OnCircuitOpenedAsync(circuit, cancellationToken);
        }
    }
}
