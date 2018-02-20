using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace TLKVIEWMODLES.Contexts.Models
{
    [Serializable]
    public class TLKENTRY
    {
        public short Type { get; set; }
        public ulong ResourceName { get; set; }
        public int Volume { get; set; }
        public int Pitch { get; set; }
        public int Offset { get; set; }
        public int Length { get; set; }

        public TLKENTRY() { }

        public TLKENTRY(byte[] buff)
        {
            Type = BitConverter.ToInt16(buff, 0);
            ResourceName = BitConverter.ToUInt64(buff, 2);
            Volume = BitConverter.ToInt32(buff, 10);
            Pitch = BitConverter.ToInt32(buff, 14);
            Offset = BitConverter.ToInt32(buff, 18);
            Length = BitConverter.ToInt32(buff, 22);
        }

        public byte[] ToByteArray()
        {
            var mstream = new MemoryStream();

            new BinaryFormatter().Serialize(mstream, this);

            return mstream.ToArray();
        }
    }
}
