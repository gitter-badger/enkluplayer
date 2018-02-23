using System.Collections.Generic;
using LightJson;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Holds all data for a schema.
    /// </summary>
    public class ElementSchemaData
    {
        /// <summary>
        /// Name to string value.
        /// </summary>
        [JsonName("strings")]
        public Dictionary<string, string> Strings = new Dictionary<string, string>();

        /// <summary>
        /// Name to int value.
        /// </summary>
        [JsonName("ints")]
        public Dictionary<string, int> Ints = new Dictionary<string, int>();

        /// <summary>
        /// Name to float value.
        /// </summary>
        [JsonName("floats")]
        public Dictionary<string, float> Floats = new Dictionary<string, float>();

        /// <summary>
        /// Name to bool value.
        /// </summary>
        [JsonName("bools")]
        public Dictionary<string, bool> Bools = new Dictionary<string, bool>();

        /// <summary>
        /// Name to Vec3 value.
        /// </summary>
        [JsonName("vectors")]
        public Dictionary<string, Vec3> Vectors = new Dictionary<string, Vec3>();

        /// <summary>
        /// Name to Vec3 value.
        /// </summary>
        [JsonName("colors")]
        public Dictionary<string, Col4> Colors = new Dictionary<string, Col4>();

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ElementSchemaData()
        {
            
        }

        /// <summary>
        /// Copy constructor.
        /// </summary>
        /// <param name="data"></param>
        internal ElementSchemaData(ElementSchemaData data)
        {
            Strings = data.Strings;
            Ints = data.Ints;
            Floats = data.Floats;
            Bools = data.Bools;
            Vectors = data.Vectors;
            Colors = data.Colors;
        }

        /// <summary>
        /// Composes another schemas data on top of this one. The composed data
        /// takes precedence.
        /// </summary>
        /// <param name="data">The data to compose.</param>
        internal void Compose(ElementSchemaData data)
        {
            if (null != data.Ints)
            {
                if (null == Ints)
                {
                    Ints = new Dictionary<string, int>();
                }

                foreach (var prop in data.Ints)
                {
                    Ints[prop.Key] = prop.Value;
                }
            }

            if (null != data.Floats)
            {
                if (null == Floats)
                {
                    Floats = new Dictionary<string, float>();
                }

                foreach (var prop in data.Floats)
                {
                    Floats[prop.Key] = prop.Value;
                }
            }

            if (null != data.Bools)
            {
                if (null == Bools)
                {
                    Bools = new Dictionary<string, bool>();
                }

                foreach (var prop in data.Bools)
                {
                    Bools[prop.Key] = prop.Value;
                }
            }

            if (null != data.Strings)
            {
                if (null == Strings)
                {
                    Strings = new Dictionary<string, string>();
                }

                foreach (var prop in data.Strings)
                {
                    Strings[prop.Key] = prop.Value;
                }
            }

            if (null != data.Vectors)
            {
                if (null == Vectors)
                {
                    Vectors = new Dictionary<string, Vec3>();
                }

                foreach (var prop in data.Vectors)
                {
                    Vectors[prop.Key] = prop.Value;
                }
            }

            if (null != data.Colors)
            {
                if (null == Colors)
                {
                    Colors = new Dictionary<string, Col4>();
                }

                foreach (var prop in data.Colors)
                {
                    Colors[prop.Key] = prop.Value;
                }
            }
        }
    }
}