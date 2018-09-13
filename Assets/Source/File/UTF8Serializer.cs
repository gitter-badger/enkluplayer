using System;
using System.Text;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// String to bytes and back.
    /// </summary>
    public class UTF8Serializer : ISerializer
    {
        /// <inheritdoc />
        public void Serialize(object value, out byte[] bytes)
        {
            bytes = Encoding.UTF8.GetBytes(value.ToString());
        }

        /// <inheritdoc />
        public void Deserialize(Type type, ref byte[] bytes, out object value)
        {
            value = Encoding.UTF8.GetString(bytes);
        }
    }
}