using System;

namespace UotanToolbox.Common.ROMHelper.SpraseHelper.Utilities.Conversion
{
    internal class Conversion
    {
        public static int ToInt32(object obj)
        {
            return ToInt32(obj, 0);
        }

        public static int ToInt32(object obj, int defaultValue)
        {
            int result = defaultValue;
            if (obj != null)
            {
                try
                {
                    result = Convert.ToInt32(obj);
                }
                catch
                { }
            }
            return result;
        }

        public static long ToInt64(object obj)
        {
            return ToInt64(obj, 0);
        }

        public static long ToInt64(object obj, long defaultValue)
        {
            long result = defaultValue;
            if (obj != null)
            {
                try
                {
                    result = Convert.ToInt64(obj);
                }
                catch
                { }
            }
            return result;
        }
    }
}
