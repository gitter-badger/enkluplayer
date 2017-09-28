using System;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Null serializer.
    /// </summary>
    public class NullSerializer : ISerializer
    {
        /// <inheritdoc cref="ISerializer"/>
        public void Serialize(object value, out byte[] bytes)
        {
            bytes = new byte[0];
        }

        /// <inheritdoc cref="ISerializer"/>
        public void Deserialize(Type type, ref byte[] bytes, out object value)
        {
            value = null;
        }
    }
}