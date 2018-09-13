using System;
using System.Collections.Generic;

namespace CreateAR.EnkluPlayer.AR
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
        /// Backing variable for property.
        /// </summary>
        private Vec3 _extents;
        
        /// <summary>
        /// Backing variable for property.
        /// </summary>
        private Vec3 _position;
        
        /// <summary>
        /// Backing variable for property.
        /// </summary>
        private Quat _rotation;
        
        /// <summary>
        /// Tags.
        /// </summary>
        public string[] Tags { get { return _tags.ToArray(); } }
        
        /// <summary>
        /// Unique id of the anchor.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Extents of the anchor in world space.
        /// </summary>
        public Vec3 Extents
        {
            get
            {
                return _extents;
            }
            internal set
            {
                if (value.Approximately(_extents))
                {
                    return;
                }

                _extents = value;

                Changed();
            }
        }

        /// <summary>
        /// World space position.
        /// </summary>
        public Vec3 Position
        {
            get
            {
                return _position;
            }
            internal set
            {
                if (value.Approximately(_position))
                {
                    return;
                }

                _position = value;

                Changed();
            }
        }
        
        /// <summary>
        /// Worldspace rotation.
        /// </summary>
        public Quat Rotation
        {
            get
            {
                return _rotation;
            }
            internal set
            {
                if (value.Approximately(_rotation))
                {
                    return;
                }

                _rotation = value;

                Changed();
            }
        }

        /// <summary>
        /// Called when the anchor has changed.
        /// </summary>
        public event Action<ArAnchor> OnChange;
        
        /// <summary>
        /// Constructor.
        /// </summary>
        public ArAnchor(string id)
        {
            Id = id;
        }

        /// <summary>
        /// ToString override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[ArAnchor Id={0}, Position=({1:0.0}, {2:0.0}, {3:0.0})]",
                Id,
                Position.x,
                Position.y,
                Position.z);
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

                Changed();
            }
        }

        /// <summary>
        /// Removes a tag.
        /// </summary>
        /// <param name="tag">Tag to remove./</param>
        public void Untag(string tag)
        {
            if (_tags.Remove(tag))
            {
                Changed();
            }
        }

        /// <summary>
        /// Clears all tags.
        /// </summary>
        public void ClearTags()
        {
            if (_tags.Count > 0)
            {
                _tags.Clear();

                Changed();
            }
        }

        /// <summary>
        /// Safely calls the OnChange event.
        /// </summary>
        private void Changed()
        {
            if (null != OnChange)
            {
                OnChange(this);
            }
        }
    }
}