using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.DataStructures;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Wraps a resizeable byte array in a pool element interface.
    /// </summary>
    public class ByteArrayHandle : IOptimizedObjectPoolElement
    {
        /// <inheritdoc />
        public int Index { get; set; }

        /// <inheritdoc />
        public bool Available { get; set; }

        /// <summary>
        /// The byte array.
        /// </summary>
        public byte[] Buffer;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="defaultSize">The number of bytes to allocate.</param>
        public ByteArrayHandle(int defaultSize)
            : this(new byte[defaultSize])
        {
            
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="buffer">The buffer to wrap.</param>
        public ByteArrayHandle(byte[] buffer)
        {
            Buffer = buffer;
        }

        /// <summary>
        /// Grows the byte array.
        /// </summary>
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