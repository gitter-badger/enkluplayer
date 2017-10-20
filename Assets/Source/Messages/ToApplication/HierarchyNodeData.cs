using Newtonsoft.Json;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    public class HierarchyNodeLocatorData
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("name")]
        public string Name;

        [JsonProperty("description")]
        public string Description;

        [JsonProperty("position")]
        public float[] PositionArray;

        [JsonProperty("rotation")]
        public float[] RotationArray;

        [JsonProperty("scale")]
        public float[] ScaleArray;

        public Vector3 Position
        {
            get
            {
                return new Vector3(
                    PositionArray[0],
                    PositionArray[1],
                    PositionArray[2]);
            }
        }

        public Vector3 Rotation
        {
            get
            {
                return new Vector3(
                    RotationArray[0],
                    RotationArray[1],
                    RotationArray[2]);
            }
        }

        public Vector3 Scale
        {
            get
            {
                return new Vector3(
                    ScaleArray[0],
                    ScaleArray[1],
                    ScaleArray[2]);
            }
        }
    }

    public class HierarchyNodeData
    {
        [JsonProperty("id")]
        public string Id;

        [JsonProperty("contentId")]
        public string ContentId;

        [JsonProperty("children")]
        public HierarchyNodeData[] Children = new HierarchyNodeData[0];

        [JsonProperty("locators")] public HierarchyNodeLocatorData[] Locators = new HierarchyNodeLocatorData[0];
    }
}