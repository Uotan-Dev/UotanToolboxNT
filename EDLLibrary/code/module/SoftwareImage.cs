using System.Collections;

namespace EDLLibrary.code.module;

public class SoftwareImage
{
    public static string RawProgramPattern => "rawprogram[0-9]{1,20}\\.xml";

    public static string PatchPattern => "patch[0-9]{1,20}\\.xml";

    public static Hashtable DummyProgress => new()
    {
        { "xbl", 1 },
        { "tz", 2 },
        { "hyp", 3 },
        { "rpm", 4 },
        { "emmc_appsboot", 5 },
        { "pmic", 6 },
        { "devcfg", 7 },
        { "BTFM", 8 },
        { "cmnlib", 9 },
        { "cmnlib64", 10 },
        { "NON-HLOS", 11 },
        { "adspso", 12 },
        { "mdtp", 13 },
        { "keymaster", 14 },
        { "misc", 15 },
        { "system", 16 },
        { "cache", 30 },
        { "userdata", 34 },
        { "recovery", 35 },
        { "splash", 36 },
        { "logo", 37 },
        { "boot", 38 },
        { "cust", 45 }
    };
}
