using System;

namespace HspcuiBase.Options
{
    [Flags]
    public enum ModeOptions
    {
        None = 0b0000,
        Debug = 0b0001,
        OnlyPreprocess = 0b0010,
        OutputUTF8 = 0b0100,
        OutputStrmap = 0b1000
    }

    [Flags]
    public enum PreprocessOptions
    {
        None = 0b0000,
        Ver26 = 0b0001,
        MakePackfile = 0b0100,
        ReadAHT = 0b1000,
        MakeAHT = 0b1_0000,
        InputUTF8 = 0b10_0000
    }

    [Flags]
    public enum Platform
    {
        x86,
        x64
    }
}
