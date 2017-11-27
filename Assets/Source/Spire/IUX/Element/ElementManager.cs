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
            
            element.OnDestroyed += ElementOnDestroyed;

            _all.Add(element);
        }
        
        /// <summary>
        /// Updates highlighted elements based on visibility.
        /// </summary>
        protected void Update()
        {
            for (var i = _all.Count - 1; i >= 0 && i < _all.Count; --i)
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
            for (var i = _all.Count - 1; i >= 0 && i < _all.Count; --i)
            {
                var element = _all[i];
                element.LateFrameUpdate();
            }
        }

        /// <summary>
        /// Invoked when a widget is destroyed
        /// </summary>
        /// <param name="element"></param>
        private void ElementOnDestroyed(IElement element)
        {
            _all.Remove(element);
        }
    }
}