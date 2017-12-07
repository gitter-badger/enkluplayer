using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Frame of reference for a locator.
    /// </summary>
    public class LocatorAnchorReferenceFrame : IAnchorReferenceFrame
    {
        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IIntentionManager _intention;
        private readonly IContentManager _content;
        
        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Forward { get { return _intention.Forward; } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Up { get { return _intention.Up; } }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public Vec3 Right { get { return _intention.Right; } }

        /// <summary>
        /// Creates a new <c>LocatorReferenceFrame</c>.
        /// </summary>
        public LocatorAnchorReferenceFrame(
            IIntentionManager intention,
            IContentManager content)
        {
            _intention = intention;
            _content = content;
        }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Attach(Anchor anchor)
        {
            var data = anchor.Data;
            var transform = anchor.transform;
            var parentTransform = transform;
            if (!string.IsNullOrEmpty(data.ContentId))
            {
                var requestedContent = _content.Request(data.ContentId);
                if (requestedContent == null)
                {
                    Log.Error(this, "Missing Content [id={0}]!", data.ContentId);
                    return;
                }

                if (!data.Reference)
                {
                    _content.Release(requestedContent);
                }

                parentTransform = requestedContent.GameObject.transform;
            }

            if (!string.IsNullOrEmpty(data.LocatorId))
            {
                parentTransform = FindExhaustive(parentTransform, data.LocatorId);
                if (parentTransform == null)
                {
                    Log.Error(this, "Missing Locator[id={0}]!", data.LocatorId);
                    return;
                }
            }

            transform.SetParent(parentTransform, true);
            transform.localPosition = Vector3.zero;
        }

        /// <inheritdoc cref="IAnchorReferenceFrame"/>
        public void Update(float dt)
        {
            //
        }

        /// <summary>
        /// Checks down and up the hierarchy for a named transform.
        /// 
        /// TODO: REMOVE THIS.
        /// </summary>
        /// <param name="root">Where to start.</param>
        /// <param name="name">The name of the transform.</param>
        /// <returns></returns>
        private Transform FindExhaustive(Transform root, string name)
        {
            if (root == null)
            {
                return null;
            }

            // first look down
            var found = FindInChildren(root, name);
            if (found != null)
            {
                return found;
            }

            // then look up
            return FindExhaustive(root.parent, name);
        }

        /// <summary>
        /// Finds a transform down the hierarchy.
        /// </summary>
        /// <param name="root">Where to start searching.</param>
        /// <param name="name">The name of the transform.</param>
        /// <returns></returns>
        private Transform FindInChildren(Transform root, string name)
        {
            if (root.name == name)
            {
                return root;
            }

            for (int i = 0, count = root.childCount; i < count; ++i)
            {
                var found = FindInChildren(root.GetChild(i), name);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }
}