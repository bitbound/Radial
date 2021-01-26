using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services
{
   public interface IJsInterop
    {
        ValueTask Alert(string message);

        void AutoHeight();

        ValueTask<bool> Confirm(string message);

        ValueTask<string> Prompt(string message);

        void StartDraggingY(string elementId, double clientY);

        void ScrollToEnd(string elementId);

    }
    public class JsInterop : IJsInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public JsInterop(IJSRuntime jSRuntime)
        {
            _jsRuntime = jSRuntime;
        }

        public ValueTask Alert(string message)
        {
            return _jsRuntime.InvokeVoidAsync("invokeAlert", message);
        }

        public void AutoHeight()
        {
            _jsRuntime.InvokeVoidAsync("autoHeight");
        }

        public ValueTask<bool> Confirm(string message)
        {
            return _jsRuntime.InvokeAsync<bool>("invokeConfirm", message);
        }

        public ValueTask<string> Prompt(string message)
        {
            return _jsRuntime.InvokeAsync<string>("invokePrompt", message);
        }
        public void ScrollToEnd(string elementId)
        {
            _jsRuntime.InvokeVoidAsync("scrollToEnd", elementId);
        }
        public void StartDraggingY(string elementId, double clientY)
        {
            _jsRuntime.InvokeVoidAsync("startDraggingY", elementId, clientY);
        }
    }
}
