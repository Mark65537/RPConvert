using System.ComponentModel;

public enum BasicFormat
{
    [Description(".png")]
    png,
    [Description(".bmp")]
    bmp,
    [Description(".jpg")]
    jpg,
    [Description(".jpeg")]
    jpeg,
    [Description(".gif")]
    gif,
    [Description(".*")]
    any
}
