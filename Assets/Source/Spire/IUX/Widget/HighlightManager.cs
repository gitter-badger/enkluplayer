using System.Collections.Generic;
using CreateAR.SpirePlayer.UI;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages IUX elements.
    /// </summary>
    public class HighlightManager : MonoBehaviour
    {
        /// <summary>
        /// Collection of only the highlighted elements.
        /// </summary>
        private readonly List<Widget> _highlighted = new List<Widget>();

        /// <summary>
        /// Retrieves the current highlighted element.
        /// 
        /// Updated every frame.
        /// </summary>
        public Widget Highlighted { get; private set; }

        /// <summary>
        /// Adds an object to highlight queue. The element with the highest
        /// HighlightPriority will be highlighted.
        /// 
        /// The Highlighted property is updated every
        /// frame, not synchronously.
        /// </summary>
        /// <param name="widget">The element to add.</param>
        public void Highlight(Widget widget)
        {
            if (_highlighted.Contains(widget))
            {
                return;
            }

            _highlighted.Add(widget);
        }

        /// <summary>
        /// Unhighlights an element.
        /// 
        /// The Highlighted property is updated every frame, not synchronously.
        /// </summary>
        /// <param name="widget">The element to remove.</param>
        public void Unhighlight(Widget widget)
        {
            _highlighted.Remove(widget);
        }
        
        /// <summary>
        /// Updates the currently highlighted element.
        /// </summary>
        private void Update()
        {
            Highlighted = null;

            var highestPriority = int.MinValue;
            for (int i = 0, count = _highlighted.Count; i < count; ++i)
            {
                var highlightable = _highlighted[i];
                if (!highlightable.IsVisible)
                {
                    continue;
                }

                if (highlightable.HighlightPriority < highestPriority)
                {
                    continue;
                }

                Highlighted = highlightable;
                highestPriority = highlightable.HighlightPriority;
            }
        }
    }
}