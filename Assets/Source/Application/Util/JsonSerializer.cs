using System;
using System.Text;
using CreateAR.Commons.Unity.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Json implementation of ISerializer.
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        /// <summary>
        /// Settings with custom converters.
        /// </summary>
        private readonly JsonSerializerSettings _settings = new JsonSerializerSettings
        {
            Converters = { new VectorConverter(), new QuaternionConverter() }
        };

        /// <inheritdoc cref="ISerializer"/>
        public virtual void Serialize(object value, out byte[] bytes)
        {
            var json = JsonConvert.SerializeObject(value, _settings);
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
                value = JsonConvert.DeserializeObject(json, type, _settings);
            }
        }
    }
}