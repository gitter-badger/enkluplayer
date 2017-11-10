using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer.AR
{
    /// <summary>
    /// Anchor data structure.
    /// </summary>
    public class ArAnchor
    {
        /// <summary>
        /// Backing variable for <c>Tags</c>.
        /// </summary>
        private readonly List<string> _tags = new List<string>();
        
        /// <summary>
        /// Tags.
        /// </summary>
        public string[] Tags { get { return _tags.ToArray(); } }
        
        /// <summary>
        /// Unique id of the anchor.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Center of the anchor in world space.
        /// </summary>
        public Vector3 Center { get; internal set; }

        /// <summary>
        /// Extents of the anchor in world space.
        /// </summary>
        public Vector3 Extents { get; internal set; }

        /// <summary>
        /// World space position.
        /// </summary>
        public Vector3 Position { get; internal set; }
        
        /// <summary>
        /// Worldspace rotation.
        /// </summary>
        public Quaternion Rotation { get; internal set; }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ArAnchor(string id)
        {
            Id = id;
        }
        
        /// <summary>
        /// Tag to add.
        /// </summary>
        /// <param name="tag">The tag to add.</param>
        public void Tag(string tag)
        {
            if (!_tags.Contains(tag))
            {
                _tags.Add(tag);
            }
        }

        /// <summary>
        /// Removes a tag.
        /// </summary>
        /// <param name="tag">Tag to remove./</param>
        public void Untag(string tag)
        {
            _tags.Remove(tag);
        }

        /// <summary>
        /// Clears all tags.
        /// </summary>
        public void ClearTags()
        {
            _tags.Clear();
        }
    }
}