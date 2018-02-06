using System;

namespace CreateAR.SpirePlayer
{
    [Serializable]
    public class PropData
    {
        public string Id = Guid.NewGuid().ToString();
        public string AssetId;

        public Vec3 Position;
        public Vec3 Rotation;
        public Vec3 Scale = Vec3.One;
        public Vec3 CenterOffset;
        public Vec3 Size;

        public bool Fade = true;

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
                AssetId = content.Data.Asset.AssetDataId,
                Position = content.GameObject.transform.position.ToVec(),
                Rotation = content.GameObject.transform.rotation.eulerAngles.ToVec(),
                Scale = content.GameObject.transform.localScale.ToVec()
            };
        }
    }
}