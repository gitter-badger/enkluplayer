using System;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Data needed for each prop in a scene.
    /// 
    /// TODO: REMOVE. We should just be using ElementData.
    /// </summary>
    [Serializable]
    public class PropData
    {
        /// <summary>
        /// Unique id of a prop.
        /// </summary>
        public string Id = Guid.NewGuid().ToString();

        /// <summary>
        /// Display name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Reference to the asset.
        /// </summary>
        public string AssetId;

        /// <summary>
        /// World space position.
        /// </summary>
        public Vec3 Position;

        /// <summary>
        /// World space rotation.
        /// </summary>
        public Vec3 Rotation;

        /// <summary>
        /// Local scale.
        /// </summary>
        public Vec3 LocalScale = Vec3.One;


        public Vec3 CenterOffset;
        public Vec3 Size;
        public bool Fade = true;

        /// <summary>
        /// Creates <c>PropData</c> from a piece of content.
        /// </summary>
        /// <param name="content">The content.</param>
        /// <returns></returns>
        public static PropData Create(ContentWidget content)
        {
            if (null == content 
                || null == content.Data
                || null == content.Data.Asset)
            {
                return null;
            }

            return new PropData
            {
                Name = content.Data.Name,
                AssetId = content.Data.Asset.AssetDataId,
                Position = content.GameObject.transform.position.ToVec(),
                Rotation = content.GameObject.transform.rotation.eulerAngles.ToVec(),
                LocalScale = content.GameObject.transform.localScale.ToVec()
            };
        }
    }
}