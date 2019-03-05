using System;
using Enklu.Data;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Anchor type for nodes.
    /// </summary>
    public enum AnchorType
    {
        None,
        Floor,
        Camera,
        Locator,
        ContextMenu,
        Sai
    }

    public enum RotationType
    {
        Preserve,
        Orient,
        Identity
    }

    public enum ScaleType
    {
        Preserve,
        Identity
    }

    /// <summary>
    /// Defines how a node is anchored.
    /// </summary>
    [Serializable]
    public class AnchorData
    {
        /// <summary>
        /// Describes how anchor functions.
        /// </summary>
        public AnchorType Type;

        /// <summary>
        /// Absolute offset.
        /// </summary>
        public Vec3 WorldOffset;

        /// <summary>
        /// View relative offset.
        /// </summary>
        public Vec3 ViewOffset;

        /// <summary>
        /// Local anchor offset.
        /// </summary>
        public Vec3 Offset;

        /// <summary>
        /// Content id.
        /// </summary>
        public string ContentId;

        /// <summary>
        /// Locator name.
        /// </summary>
        public string LocatorId;

        /// <summary>
        /// If true, orients to camera forward.
        /// </summary>
        public RotationType Rotation = RotationType.Preserve;

        /// <summary>
        /// If true, orients to camera forward.
        /// </summary>
        public ScaleType Scale = ScaleType.Preserve;

        /// <summary>
        /// If true, anchors to location defined by spatial understanding.
        /// </summary>
        public bool Spatial;

        /// <summary>
        /// Reference new content.
        /// </summary>
        public bool Reference = true;
    }
}
