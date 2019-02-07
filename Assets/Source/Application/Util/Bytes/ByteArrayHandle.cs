using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.DataStructures;

namespace CreateAR.EnkluPlayer
{
    public class ByteArrayHandle : IOptimizedObjectPoolElement
    {
        public int Index { get; set; }
        public bool Available { get; set; }

        public byte[] Buffer;

        public ByteArrayHandle(int defaultSize)
            : this(new byte[defaultSize])
        {
            
        }

        public ByteArrayHandle(byte[] buffer)
        {
            Buffer = buffer;
        }

        public void Grow()
        {
            var currentSize = Buffer.Length;
            var targetSize = currentSize * 2;

            Log.Debug(this, "Grow buffer from {0} to {1}.",
                currentSize,
                targetSize);

            var buffer = new byte[targetSize];
            Array.Copy(Buffer, 0, buffer, 0, currentSize);
            Buffer = buffer;
        }
    }
}