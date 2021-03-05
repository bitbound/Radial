using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Radial.Services.Client
{
   public interface IJsInterop
    {
        void AddClassName(ElementReference element, string className);
        ValueTask Alert(string message);

        void AutoHeight();

        ValueTask<bool> Confirm(string message);

        ValueTask<string> Prompt(string message);
        void SetStyleProperty(ElementReference element, string propertyName, string value);
        void StartDraggingY(ElementReference element, double clientY);

        void ScrollToEnd(ElementReference element);

        void AddBeforeUnloadHandler();

    }
    public class JsInterop : IJsInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public JsInterop(IJSRuntime jSRuntime)
        {
            _jsRuntime = jSRuntime;
        }

        public void AddBeforeUnloadHandler()
        {
            _jsRuntime.InvokeVoidAsync("addBeforeUnloadHandler");
        }

        public void AddClassName(ElementReference element, string className)
        {
            _jsRuntime.InvokeVoidAsync("addClassName", element, className);
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
        public void ScrollToEnd(ElementReference element)
        {
            _jsRuntime.InvokeVoidAsync("scrollToEnd", element);
        }
        public void SetStyleProperty(ElementReference element, string propertyName, string value)
        {
            _jsRuntime.InvokeVoidAsync("setStyleProperty", element, propertyName, value);
        }
        public void StartDraggingY(ElementReference element, double clientY)
        {
            _jsRuntime.InvokeVoidAsync("startDraggingY", element, clientY);
        }
    }
}
