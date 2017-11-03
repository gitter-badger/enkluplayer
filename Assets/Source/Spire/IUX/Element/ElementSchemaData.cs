using System.Collections.Generic;

namespace CreateAR.SpirePlayer.UI
{
    public class ElementSchemaData
    {
        public Dictionary<string, string> Strings;
        public Dictionary<string, int> Ints;
        public Dictionary<string, float> Floats;
        public Dictionary<string, bool> Bools;
        public Dictionary<string, Vec3> Vectors;

        public ElementSchemaData()
        {
            
        }

        public ElementSchemaData(ElementSchemaData data)
        {
            Strings = data.Strings;
            Ints = data.Ints;
            Floats = data.Floats;
            Bools = data.Bools;
            Vectors = data.Vectors;
        }

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