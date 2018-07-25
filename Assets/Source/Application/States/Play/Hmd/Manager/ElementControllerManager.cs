using System.Collections;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Http;
using CreateAR.SpirePlayer.IUX;

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
        private readonly List<ElementControllerGroup> _groups = new List<ElementControllerGroup>();

        /// <summary>
        /// Manages application scenes.
        /// </summary>
        private readonly IAppSceneManager _scenes;

        /// <summary>
        /// Intention.
        /// </summary>
        private readonly IIntentionManager _intention;
        
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
        public ElementControllerManager(
            IAppSceneManager scenes,
            IIntentionManager intention,
            IBootstrapper bootstrapper)
        {
            _scenes = scenes;
            _intention = intention;
            
            bootstrapper.BootstrapCoroutine(Update());
        }
        
        /// <inheritdoc />
        public IElementControllerGroup Group(string tag)
        {
            ElementControllerGroup group;
            for (int i = 0, len = _groups.Count; i < len; i++)
            {
                group = _groups[i];
                if (group.Tag == tag)
                {
                    return group;
                }
            }

            // create!
            group = new ElementControllerGroup(_scenes, tag)
            {
                Active = true
            };
            _groups.Add(group);

            return group;
        }

        /// <inheritdoc />
        public void Deactivate(params string[] tags)
        {
            for (var i = _groups.Count - 1; i >= 0; i--)
            {
                var group = _groups[i];
                for (int j = 0, jlen = tags.Length; j < jlen; j++)
                {
                    if (group.Tag == tags[j])
                    {
                        group.Active = false;
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Activate(params string[] tags)
        {
            for (var i = _groups.Count - 1; i >= 0; i--)
            {
                var group = _groups[i];
                for (int j = 0, jlen = tags.Length; j < jlen; j++)
                {
                    if (group.Tag == tags[j])
                    {
                        group.Active = true;
                        break;
                    }
                }
            }
        }

        /// <inheritdoc />
        public void Release()
        {
            for (var i = _groups.Count - 1; i >= 0; i--)
            {
                _groups[i].Destroy();
            }

            _groups.Clear();
        }

        /// <summary>
        /// Called every frame to update groups.
        /// </summary>
        /// <returns></returns>
        private IEnumerator Update()
        {
            while (true)
            {
                var origin = _intention.Origin.ToVector();
                var direction = _intention.Forward.ToVector();

                for (int i = 0, len = _groups.Count; i < len; i++)
                {
                    _groups[i].Update(origin, direction);
                }

                yield return null;
            }
        }
    }
}