using LightJson;

namespace CreateAR.SpirePlayer
{
    public class HierarchyNodeLocatorData
    {
        [JsonName("id")]
        public string Id;

        [JsonName("name")]
        public string Name;

        [JsonName("description")]
        public string Description;

        [JsonName("position")]
        public float[] PositionArray;

        [JsonName("rotation")]
        public float[] RotationArray;

        [JsonName("scale")]
        public float[] ScaleArray;

        public Vec3 Position
        {
            get
            {
                return new Vec3(
                    PositionArray[0],
                    PositionArray[1],
                    PositionArray[2]);
            }
        }

        public Vec3 Rotation
        {
            get
            {
                return new Vec3(
                    RotationArray[0],
                    RotationArray[1],
                    RotationArray[2]);
            }
        }

        public Vec3 Scale
        {
            get
            {
                return new Vec3(
                    ScaleArray[0],
                    ScaleArray[1],
                    ScaleArray[2]);
            }
        }
    }

    public class HierarchyNodeData
    {
        [JsonName("id")]
        public string Id;

        [JsonName("contentId")]
        public string ContentId;

        [JsonName("children")]
        public HierarchyNodeData[] Children = new HierarchyNodeData[0];

        [JsonName("locators")] public HierarchyNodeLocatorData[] Locators = new HierarchyNodeLocatorData[0];
    }
}