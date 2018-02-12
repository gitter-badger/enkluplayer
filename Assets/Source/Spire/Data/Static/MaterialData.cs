using System.Text;
using LightJson;

namespace CreateAR.SpirePlayer
{
    public class MaterialTextureData
    {
        /// <summary>
        /// Uniform.
        /// </summary>
        [JsonName("uniform")]
        public string Uniform;

        /// <summary>
        /// Texture asset id.
        /// </summary>
        [JsonName("assetId")]
        public string AssetId;
    }

    public class MaterialPropertyData
    {
        /// <summary>
        /// Uniform.
        /// </summary>
        [JsonName("uniform")]
        public string Uniform;

        /// <summary>
        /// Texture asset id.
        /// </summary>
        [JsonName("value")]
        public string Value;
    }

    /// <summary>
    /// Data structure for materials.
    /// </summary>
    public class MaterialData : StaticData
    {
        /// <summary>
        /// Id of the shader to use.
        /// </summary>
        [JsonName("shaderId")]
        public string ShaderId;

        /// <summary>
        /// Uniform => Value.
        /// </summary>
        [JsonName("properties")]
        public MaterialPropertyData[] Properties;

        /// <summary>
        /// Uniform => AssetId.
        /// </summary>
        [JsonName("textures")]
        public MaterialTextureData[] Textures;

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

            foreach (var texture in Textures)
            {
                builder.AppendFormat(" {0}={1} ",
                    texture.Uniform,
                    texture.AssetId);
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
                    pair.Uniform,
                    pair.Value);
            }

            return builder.ToString();
        }
    }
}