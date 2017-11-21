using CreateAR.Commons.Unity.Messaging;
using CreateAR.SpirePlayer.UI;
using System.Collections.Generic;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Manages IUX elements.
    /// </summary>
    public class ElementManager : InjectableMonoBehaviour, IElementManager
    {
        /// <summary>
        /// Collection of all elements.
        /// </summary>
        private readonly List<IElement> _all = new List<IElement>();

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
            
            element.OnDestroy += Element_OnDestroy;

            _all.Add(element);
        }

        /// <summary>
        /// Returns the number of visible elements of type T.
        /// </summary>
        /// <returns></returns>
        /*public int GetVisibleCount<T>()
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
        }*/

        /// <summary>
        /// Updates highlighted elements based on visibility.
        /// </summary>
        protected void Update()
        {
            for (int i = _all.Count - 1; i >= 0 && i < _all.Count; --i)
            {
                var element = _all[i];
                element.FrameUpdate();
            }
        }

        /// <summary>
        /// Updates highlighted elements based on visibility.
        /// </summary>
        protected void LateUpdate()
        {
            for (int i = _all.Count - 1; i >= 0 && i < _all.Count; --i)
            {
                var element = _all[i];
                element.LateFrameUpdate();
            }
        }

        /// <summary>
        /// Invoked when a widget is destroyed
        /// </summary>
        /// <param name="element"></param>
        private void Element_OnDestroy(IElement element)
        {
            _all.Remove(element);
        }
    }
}