using Microsoft.JSInterop;

namespace EHive.Admin.Helpers
{
    public class ClipboardHelper : IClipboardHelper
    {
        private readonly IJSRuntime jsRuntime;

        public ClipboardHelper(IJSRuntime jsRuntime)
        {
            this.jsRuntime = jsRuntime;
        }

        public ValueTask CopyToClipboard(string text)
        {
            return jsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", text);
        }
    }
}