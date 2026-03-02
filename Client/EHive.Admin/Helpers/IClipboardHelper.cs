
namespace EHive.Admin.Helpers
{
    public interface IClipboardHelper
    {
        ValueTask CopyToClipboard(string text);
    }
}