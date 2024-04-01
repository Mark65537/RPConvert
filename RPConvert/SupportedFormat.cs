using System.ComponentModel;

public enum SupportedFormat
{
    [Description(".png")]
    Png,
    [Description(".bmp")]
    Bmp,
    [Description(".jpg")]
    Jpg,
    [Description(".jpeg")]
    Jpeg,
    [Description(".gif")]
    Gif,
    [Description(".*")]
    Any
}
