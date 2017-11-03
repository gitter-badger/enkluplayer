using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
{
    /// <summary>
    /// Holds all data for a schema.
    /// </summary>
    public class ElementSchemaData
    {
        /// <summary>
        /// Name to string value.
        /// </summary>
        public Dictionary<string, string> Strings;

        /// <summary>
        /// Name to int value.
        /// </summary>
        public Dictionary<string, int> Ints;

        /// <summary>
        /// Name to float value.
        /// </summary>
        public Dictionary<string, float> Floats;

        /// <summary>
        /// Name to bool value.
        /// </summary>
        public Dictionary<string, bool> Bools;

        /// <summary>
        /// Name to Vec3 value.
        /// </summary>
        public Dictionary<string, Vec3> Vectors;

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
        }
    }
}