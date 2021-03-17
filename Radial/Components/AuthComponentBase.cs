using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Radial.Data.Entities;

namespace Radial.Components
{
    public interface IAuthComponentBase
    {
        bool IsAuthenticated { get; }
        RadialUser User { get; }
        string Username { get; }
    }
    public class AuthComponentBase : ComponentBase, IAuthComponentBase
    {
        public bool IsAuthenticated => GetAuthState()?.User?.Identity?.IsAuthenticated ?? false;

        public RadialUser User => UserManager?.GetUserAsync(GetAuthState()?.User)?.GetAwaiter().GetResult();

        public string Username => GetAuthState()?.User?.Identity?.Name;

        [Inject]
        protected AuthenticationStateProvider AuthStateProvider { get; set; }

        [Inject]
        protected UserManager<RadialUser> UserManager { get; set; }

        private AuthenticationState GetAuthState()
        {
            return AuthStateProvider?.GetAuthenticationStateAsync()?.GetAwaiter().GetResult();
        }
    }
}
