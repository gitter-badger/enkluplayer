﻿using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Data structure for materials.
    /// </summary>
    public class MaterialData : StaticData
    {
        /// <summary>
        /// Id of the shader to use.
        /// </summary>
        [JsonProperty("shaderId")]
        public string ShaderId { get; set; }

        /// <summary>
        /// Uniform => Value.
        /// </summary>
        [JsonProperty("properties")]
        public Dictionary<string, object> Properties;

        /// <summary>
        /// Uniform => AssetId.
        /// </summary>
        [JsonProperty("textures")]
        public Dictionary<string, string> Textures;

        /// <summary>
        /// Useful ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[MaterialData Id={0}, Props=[{1}], Textures=[{2}]]",
                Id,
                PropertyString(),
                TextureString());
        }

        /// <summary>
        /// Creates a useful string from the textures dictionary.
        /// </summary>
        /// <returns></returns>
        private string TextureString()
        {
            var builder = new StringBuilder();

            foreach (var pair in Textures)
            {
                builder.AppendFormat(" {0}={1} ",
                    pair.Key,
                    pair.Value);
            }

            return builder.ToString();
        }

        /// <summary>
        /// Creates a string from a property dictionary.
        /// </summary>
        /// <returns></returns>
        private string PropertyString()
        {
            var builder = new StringBuilder();

            foreach (var pair in Properties)
            {
                builder.AppendFormat(" {0}={1} ",
                    pair.Key,
                    pair.Value);
            }

            return builder.ToString();
        }
    }
}