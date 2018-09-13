using UnityEngine;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// For defining relative position between two widgets
    /// </summary>
    public enum WidgetAnchorPosition
    {
        Bottom,
        Right,
        Top,
        Left,
        Center
    }

    /// <summary>
    /// Dynamically adjusts widget position by setting parent
    /// to pre-defined relative offset transform
    /// </summary>
    public class WidgetAnchors : MonoBehaviour
    {
        /// <summary>
        /// Transform above the widget
        /// </summary>
        public Transform Top;

        /// <summary>
        /// Transform below the widget
        /// </summary>
        public Transform Bottom;

        /// <summary>
        /// Transform to the right of the widget
        /// </summary>
        public Transform Right;

        /// <summary>
        /// Transform to the left of the widget
        /// </summary>
        public Transform Left;

        /// <summary>
        /// Transform in the center of the widget
        /// </summary>
        public Transform Center;

        /// <summary>
        /// Refresh the relative parent
        /// </summary>
        public void Anchor(Transform targetTransform, WidgetAnchorPosition widgetAnchorPosition)
        {
            var parentTransform = GetParentTransform(widgetAnchorPosition);
            if (targetTransform.parent != parentTransform)
            {
                targetTransform.parent = parentTransform;
                targetTransform.localPosition = Vector3.zero;
            }
        }

        /// <summary>
        /// Returns the parent transform
        /// </summary>
        /// <param name="widgetAnchorPosition"></param>
        /// <returns></returns>
        private Transform GetParentTransform(WidgetAnchorPosition widgetAnchorPosition)
        {
            switch (widgetAnchorPosition)
            {
                case WidgetAnchorPosition.Center:
                    return Center;
                case WidgetAnchorPosition.Bottom:
                    return Bottom;
                case WidgetAnchorPosition.Top:
                    return Top;
                case WidgetAnchorPosition.Right:
                    return Right;
                case WidgetAnchorPosition.Left:
                    return Left;
            }

            return null;
        }
    }
}