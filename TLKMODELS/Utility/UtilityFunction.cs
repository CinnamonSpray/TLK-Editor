using System;
using System.Runtime.InteropServices;

namespace TLK.Utility
{
    internal sealed class Converting
    {
        public T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            T stuff;
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);

            /*
            try
            {
                stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            }
            finally
            {
                handle.Free();
            }
            */

            stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(T));
            handle.Free();

            return stuff;
        }
        /*
        public unsafe T UnsafeByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            fixed (byte* ptr = &bytes[0])
            {
                return (T)Marshal.PtrToStructure((IntPtr)ptr, typeof(T));
            }
        }
        */
    }
}
