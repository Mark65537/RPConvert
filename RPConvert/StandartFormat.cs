﻿using System.ComponentModel;

internal enum StandartFormat
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
