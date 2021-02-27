using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace Radial.Services.Client
{
    public interface IAppState : INotifyPropertyChanged
    {
        bool IsPowersMenuOpen { get; set; }
    }

    public class AppState : IAppState
    {
        private bool _isPowersMenuOpen;

        public event EventHandler PowersWindowOpened;
        public event PropertyChangedEventHandler PropertyChanged;

        public bool IsPowersMenuOpen
        {
            get
            {
                return _isPowersMenuOpen;
            }
            set
            {
                if (value == _isPowersMenuOpen)
                {
                    return;
                }

                _isPowersMenuOpen = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
