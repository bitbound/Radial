using Radial.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Radial.Services.Client
{
    public interface IToastService
    {
        List<Toast> Toasts { get; }

        event EventHandler OnToastsChanged;

        void ShowToast(string message, int expirationMillisecond = 3000, string classString = null, string styleOverrides = null);
    }

    public class ToastService : IToastService
    {
        public event EventHandler OnToastsChanged;
        public List<Toast> Toasts => ToastCache.Values.ToList();
        private ConcurrentDictionary<string, Toast> ToastCache { get; } = new ConcurrentDictionary<string, Toast>();

        private Timer ClearToastsTimer { get; set; }


        public void ShowToast(string message,
            int expirationMillisecond = 3000,
            string classString = null,
            string styleOverrides = null)
        {

            if (string.IsNullOrWhiteSpace(classString))
            {
                classString = "bg-success text-white";
            };

            var toastModel = new Toast(Guid.NewGuid().ToString(), 
                message,
                classString, 
                TimeSpan.FromMilliseconds(expirationMillisecond),
                styleOverrides);

            ToastCache.AddOrUpdate(toastModel.Guid, toastModel, (k, v) => toastModel);
            OnToastsChanged?.Invoke(this, null);

            ClearToastsTimer?.Dispose();
            ClearToastsTimer = new Timer(ToastCache.Values.Max(x => x.Expiration.TotalMilliseconds) + 5000)
            {
                AutoReset = false
            };
            ClearToastsTimer.Elapsed += (s, e) =>
            {
                ToastCache.Clear();
                OnToastsChanged?.Invoke(this, null);
            };
            ClearToastsTimer.Start();
        }
    }
}
