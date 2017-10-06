using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using CreateAR.Commons.Unity.Http;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Serializer that uses System.Xml serializer.
    /// </summary>
    public class SystemXmlSerializer : ISerializer
    {
        /// <summary>
        /// Cache serializers.
        /// </summary>
        private readonly Dictionary<Type, XmlSerializer> _serializers = new Dictionary<Type, XmlSerializer>();

        /// <inheritdoc cref="ISerializer"/>
        public void Serialize(object value, out byte[] bytes)
        {
            var serializer = Serializer(value.GetType());
            using (var stream = new MemoryStream())
            {
                serializer.Serialize(stream, value);

                bytes = stream.ToArray();
            }
        }

        /// <inheritdoc cref="ISerializer"/>
        public void Deserialize(Type type, ref byte[] bytes, out object value)
        {
            var serializer = Serializer(type);
            using (var stream = new MemoryStream(bytes))
            {
                value = serializer.Deserialize(stream);
            }
        }

        /// <summary>
        /// Creates or retrieves a cached version of a serializer for a type/
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        private XmlSerializer Serializer(Type type)
        {
            XmlSerializer serializer;
            if (!_serializers.TryGetValue(type, out serializer))
            {
                serializer = _serializers[type] = new XmlSerializer(type);
            }

            return serializer;
        }
    }
}