
namespace OnHive.Admin.Helpers
{
    public interface IClipboardHelper
    {
        ValueTask CopyToClipboard(string text);
    }
}