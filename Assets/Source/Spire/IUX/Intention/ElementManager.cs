using System.Collections.Generic;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages IUX elements.
    /// </summary>
    public class ElementManager : MonoBehaviour
    {
        /// <summary>
        /// Collection of all elements.
        /// </summary>
        private readonly List<IElement> _all = new List<IElement>();

        /// <summary>
        /// Collection of only the highlighted elements.
        /// </summary>
        private readonly List<IElement> _highlighted = new List<IElement>();

        /// <summary>
        /// Retrieves the current highlighted element.
        /// 
        /// Updated every frame.
        /// </summary>
        public IElement Highlighted { get; private set; }

        /// <summary>
        /// Adds an element. Should be called when object is created.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Add(IElement element)
        {
            if (_all.Contains(element))
            {
                return;
            }

            _all.Add(element);
        }

        /// <summary>
        /// Removes an element from management. Should be called when object
        /// is destroyed.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public void Remove(IElement element)
        {
            _all.Remove(element);
        }

        /// <summary>
        /// Adds an object to highlight queue. The element with the highest
        /// HighlightPriority will be highlighted.
        /// 
        /// The Highlighted property is updated every
        /// frame, not synchronously.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Highlight(IElement element)
        {
            if (_highlighted.Contains(element))
            {
                return;
            }

            _highlighted.Add(element);
        }

        /// <summary>
        /// Unhighlights an element.
        /// 
        /// The Highlighted property is updated every frame, not synchronously.
        /// </summary>
        /// <param name="element">The element to remove.</param>
        public void Unhighlight(IElement element)
        {
            _highlighted.Remove(element);
        }

        /// <summary>
        /// Returns the number of visible elements of type T.
        /// </summary>
        /// <returns></returns>
        public int GetVisibleCount<T>()
        {
            var visibleCount = 0;

            for (int i = 0, count = _all.Count; i < count; ++i)
            {
                var element = _all[i];
                if (element.IsVisible && element is T)
                {
                    visibleCount++;
                }
            }

            return visibleCount;
        }

        /// <summary>
        /// Updates highlighted elements based on visibility.
        /// </summary>
        private void Update()
        {
            UpdateHighlighted();
        }
        
        /// <summary>
        /// Updates the currently highlighted element.
        /// </summary>
        private void UpdateHighlighted()
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