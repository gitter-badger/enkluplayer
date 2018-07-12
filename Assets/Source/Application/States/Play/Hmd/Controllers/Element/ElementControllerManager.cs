using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Basic implementation of <c>IElementControllerManager</c> that also acts as a factory.
    /// </summary>
    public class ElementControllerManager : IElementControllerManager
    {
        /// <summary>
        /// Set of groups, which themselves manage controllers.
        /// </summary>
        private readonly List<IElementControllerGroup> _groups = new List<IElementControllerGroup>();

        /// <summary>
        /// Manages application scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Backing variable for Active property.
        /// </summary>
        private bool _isActive;

        /// <inheritdoc />
        public bool Active
        {
            get
            {
                return _isActive;
            }
            set
            {
                _isActive = value;

                for (int i = 0, len = _groups.Count; i < len; i++)
                {
                    _groups[i].Active = _isActive;
                }
            }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementControllerManager(IAppSceneManager scenes)
        {
            _scenes = scenes;
        }

        /// <inheritdoc />
        public IElementControllerGroup Group(string tag)
        {
            IElementControllerGroup group;
            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                group = _groups[i];
                if (group.Tag == tag)
                {
                    return group;
                }
            }

            // create!
            group = new ElementControllerGroup(_scenes, tag);
            _groups.Add(group);

            return group;
        }

        /// <inheritdoc />
        public void Destroy(params string[] tags)
        {
            for (var i = _groups.Count - 1; i >= 0; i--)
            {
                var group = _groups[i];
                for (int j = 0, jlen = tags.Length; j < jlen; j++)
                {
                    if (group.Tag == tags[j])
                    {
                        group.Destroy();

                        _groups.RemoveAt(i);
                        break;
                    }
                }
            }
        }
    }
}