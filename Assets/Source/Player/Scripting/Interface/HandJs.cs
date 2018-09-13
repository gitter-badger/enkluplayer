using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// Surfaces properties that are specific to the user's hand.
    /// </summary>
    public class HandJs : IEntityJs
    {
        /// <summary>
        /// Backing transform helper.
        /// </summary>
        private readonly UnityTransformJsApi _unityTransform;

        /// <summary>
        /// Backing GameObject for the Hand.
        /// </summary>
        public readonly GameObject gameObject;

        /// <summary>
        /// Transform.
        /// </summary>
        public IElementTransformJsApi transform
        {
            get { return _unityTransform; }
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="gameObject"></param>
        public HandJs(GameObject gameObject)
        {
            this.gameObject = gameObject;
            _unityTransform = new UnityTransformJsApi(gameObject.transform);
        }

        /// <summary>
        /// Always returns false, since the Hand can't belong to the hierarchy.
        /// </summary>
        /// <param name="parent"></param>
        /// <returns></returns>
        public bool isChildOf(IEntityJs parent)
        {
            return false;
        }

        /// <summary>
        /// Sets the transforms local position and updates the JS transform values.
        /// </summary>
        /// <param name="position"></param>
        [DenyJsAccess]
        public void UpdatePosition(Vector3 position)
        {
            _unityTransform.UnityTransform.localPosition = position;
            _unityTransform.UpdateJsTransform();
        }
    }
}
