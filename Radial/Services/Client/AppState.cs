using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Radial.Services.Client
{
    public interface IAppState : INotifyPropertyChanged
    {
        bool IsPowersMenuOpen { get; set; }
    }

    public class AppState : IAppState
    {
        private bool _isPowersMenuOpen;

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
