using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.Commons.Unity.Logging;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Simple implementation of IUIElementFactory.
    /// 
    /// This is NOT a long-term class! UI should be bundled and streamed in.
    /// </summary>
    public class SceneUIElementFactory : InjectableMonoBehaviour, IUIElementFactory
    {
        /// <summary>
        /// Data structure to link UIDataId to prefab.
        /// </summary>
        [Serializable]
        public class ElementLink
        {
            /// <summary>
            /// UIData id.
            /// </summary>
            public string UIDataId;

            /// <summary>
            /// Element to instantiate.
            /// </summary>
            public MonoBehaviourUIElement Element;
        }

        /// <summary>
        /// A library of links for a set of platforms.
        /// </summary>
        [Serializable]
        public class ElementLinkLibrary
        {
            /// <summary>
            /// Platforms supported.
            /// </summary>
            public RuntimePlatform[] Platforms;

            /// <summary>
            /// Links.
            /// </summary>
            public ElementLink[] Links;
        }

        /// <summary>
        /// Root Transform.
        /// </summary>
        private Transform _arRoot;

        /// <summary>
        /// Application-wide configuration.
        /// </summary>
        [Inject]
        public ApplicationConfig Config { get; set; }

        [Inject]
        public IDesignController Designer { get; set; }

        /// <summary>
        /// All libraries.
        /// </summary>
        public ElementLinkLibrary[] Libraries;

        /// <inheritdoc />
        public IAsyncToken<IUIElement> Element(UIReference reference, int id)
        {
            var link = Link(reference);
            if (null == link)
            {
                return new AsyncToken<IUIElement>(new Exception(string.Format("No element for id {0}.", reference.UIDataId)));
            }

            var instance = Instantiate(link.Element, GetRoot());
            instance.Init(id);

            return new AsyncToken<IUIElement>(instance);
        }

        /// <summary>
        /// Retrieves an appropriate root.
        /// </summary>
        /// <returns></returns>
        private Transform GetRoot()
        {
            // TODO: There should be a child for each controller implementation.
            if (Designer is HmdDesignController)
            {
                if (null != _arRoot)
                {
                    return _arRoot;
                }

                var root = new GameObject("IUX Root");
                root.transform.position = Vector3.zero;
                root.transform.rotation = Quaternion.identity;

                _arRoot = root.transform;

                return _arRoot;
            }

            return transform;
        }

        /// <summary>
        /// Retrieves a link for a reference.
        /// </summary>
        /// <param name="reference">The reference.</param>
        /// <returns></returns>
        private ElementLink Link(UIReference reference)
        {
            var lib = Library();
            if (null == lib)
            {
                return null;
            }

            for (var i = 0; i < lib.Links.Length; i++)
            {
                var link = lib.Links[i];
                if (link.UIDataId == reference.UIDataId)
                {
                    return link;
                }
            }

            return null;
        }

        /// <summary>
        /// Retrieves the library for the current platform.
        /// </summary>
        /// <returns></returns>
        private ElementLinkLibrary Library()
        {
            for (int i = 0, len = Libraries.Length; i < len; i++)
            {
                var lib = Libraries[i];
                for (var j = 0; j < lib.Platforms.Length; j++)
                {
                    var platform = lib.Platforms[j];
                    if (platform == Config.ParsedPlatform)
                    {
                        return lib;
                    }
                }
            }

            Log.Warning(this, "No ElementLinkLibrary for platform {0}.", Config.ParsedPlatform);

            return null;
        }
    }
}