using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Shared
{
    public interface IAuthComponentBase
    {
        bool IsAuthenticated { get; }
        string Username { get; }
    }
    public class AuthComponentBase : ComponentBase, IAuthComponentBase
    {
        public bool IsAuthenticated => GetAuthState()?.User?.Identity?.IsAuthenticated ?? false;

        public string Username => GetAuthState()?.User?.Identity?.Name;

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

        private AuthenticationState GetAuthState()
        {
            return AuthStateProvider?.GetAuthenticationStateAsync()?.GetAwaiter().GetResult();
        }
    }
}
