using System.Runtime.InteropServices;

namespace DiskPartitionInfo.Extensions
{
    internal static class ByteArrayExtensions
    {
        internal static T ToStruct<T>(this byte[] bytes)
            where T : struct
        {
            T result;
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            try
            {
                result = (T) Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T))!;
            }
            finally
            {
                handle.Free();
            }

            return result;
        }
    }
}
