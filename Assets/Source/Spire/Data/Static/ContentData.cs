using System;
using System.Collections.Generic;
using LightJson;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Associates AssetData, AnchorData, and some data about the Content.
    /// </summary>
    [Serializable]
    public class ContentData : StaticData
    {
        /// <summary>
        /// If true, each request from the IContentManager will create a new
        /// <c>Content</c> instance.
        /// </summary>
        [JsonName("unique")]
        public bool Unique;
        
        /// <summary>
        /// Tags associated with this piece of content.
        /// </summary>
        [JsonName("tags")]
        public string Tags;

        /// <summary>
        /// Data about the asset.
        /// </summary>
        [JsonName("asset")]
        public AssetReference Asset;

        /// <summary>
        /// Links to the material.
        /// </summary>
        [JsonName("materialId")]
        public string MaterialId;
        
        /// <summary>
        /// Describes how to anchor this content.
        /// </summary>
        [JsonName("anchor")]
        public AnchorData Anchor = new AnchorData();

        /// <summary>
        /// Scripts that execute on this piece of content.
        /// </summary>
        [JsonName("scripts")]
        public ScriptReference[] Scripts = new ScriptReference[0];

        /// <summary>
        /// Useful ToString.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ContentData Id={0} Asset={1}]",
                Id,
                Asset);
        }
    }
}