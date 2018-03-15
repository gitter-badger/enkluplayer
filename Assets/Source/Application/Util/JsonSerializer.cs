﻿using System;
using System.Text;
using CreateAR.Commons.Unity.Http;
using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Json implementation of ISerializer.
    /// </summary>
    public class JsonSerializer : ISerializer
    {
        /// <inheritdoc cref="ISerializer"/>
        public void Serialize(object value, out byte[] bytes)
        {
            bytes = Encoding.UTF8.GetBytes(new JsonObject(value).ToString(true));
        }

        /// <inheritdoc cref="ISerializer"/>
        public void Deserialize(Type type, ref byte[] bytes, out object value)
        {
            var json = Encoding.UTF8.GetString(bytes);
            if (string.IsNullOrEmpty(json))
            {
                value = null;
            }
            else
            {
                value = JsonValue
                    .Parse(json)
                    .As(type);
            }
        }
    }
}