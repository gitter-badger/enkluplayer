using System;
using System.Text;
using CreateAR.Commons.Unity.Http;
using Newtonsoft.Json;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Json implementation of ISerializer.
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        /// <inheritdoc cref="ISerializer"/>
        public virtual void Serialize(object value, out byte[] bytes)
        {
            var json = JsonConvert.SerializeObject(value);
            bytes = Encoding.UTF8.GetBytes(json);
        }

        /// <inheritdoc cref="ISerializer"/>
        public virtual void Deserialize(Type type, ref byte[] bytes, out object value)
        {
            var json = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrEmpty(json))
            {
                value = null;
            }
            else
            {
                value = JsonConvert.DeserializeObject(json, type);
            }
        }
    }
}