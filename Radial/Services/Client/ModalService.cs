using Microsoft.AspNetCore.Components;
using Radial.Components.Modals;
using Radial.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Radial.Services.Client
{
    public interface IModalService
    {
        event EventHandler ModalShown;
        List<ModalButton> Buttons { get; }
        string[] Body { get; }
        string TextInput { get; set; }
        string Title { get; }
        Task ShowModal(string title, string[] body, ModalButton[] buttons = null);
    }

    public class ModalService : IModalService
    {
        private readonly SemaphoreSlim _modalLock = new SemaphoreSlim(1, 1);

        public event EventHandler ModalShown;
        public List<ModalButton> Buttons { get; } = new List<ModalButton>();
        public string[] Body { get; private set; }
        public bool ShowInput { get; private set; }
        public string TextInput { get; set; }
        public string Title { get; private set; }
        public async Task ShowModal(string title, string[] body, ModalButton[] buttons = null)
        {
            try
            {
                await _modalLock.WaitAsync();
                Title = title;
                Body = body;
                Buttons.Clear();
                if (buttons is not null)
                {
                    Buttons.AddRange(buttons);
                }
                ModalShown?.Invoke(this, null);
            }
            finally
            {
                _modalLock.Release();
            }
        }
    }
}
