using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services.Client
{
    public interface IUiNoticiations
    {
        event EventHandler PowersWindowOpened;
        void InvokePowersMenuOpenedEvent();
    }

    public class UiNotifications : IUiNoticiations
    {
        public event EventHandler PowersWindowOpened;

        public void InvokePowersMenuOpenedEvent()
        {
            PowersWindowOpened?.Invoke(this, null);
        }
    }
}
