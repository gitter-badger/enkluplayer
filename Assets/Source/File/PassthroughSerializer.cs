using System;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Passes bytes through.
    /// </summary>
    public class PassthroughSerializer : ISerializer
    {
        /// <inheritdoc />
        public void Serialize(object value, out byte[] bytes)
        {
            bytes = (byte[]) value;
        }

        /// <inheritdoc />
        public void Deserialize(Type type, ref byte[] bytes, out object value)
        {
            value = bytes;
        }
    }
}