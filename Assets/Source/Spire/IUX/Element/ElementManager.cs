using System.Collections.Generic;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Manages IUX elements.
    /// </summary>
    public class ElementManager : InjectableMonoBehaviour, IElementManager
    {
        /// <summary>
        /// Collection of all elements.
        /// </summary>
        private readonly List<Element> _all = new List<Element>();

        /// <summary>
        /// Adds an element. Should be called when object is created.
        /// </summary>
        /// <param name="element">The element to add.</param>
        public void Add(Element element)
        {
            if (_all.Contains(element))
            {
                return;
            }
            
            element.OnDestroyed += ElementOnDestroyed;

            _all.Add(element);
        }

        /// <summary>
        /// Retrieves an <c>Element</c> by Guid.
        /// </summary>
        /// <param name="guid">Unique guid.</param>
        /// <returns></returns>
        public Element ByGuid(string guid)
        {
            for (int i = 0, len = _all.Count; i < len; i++)
            {
                var element = _all[i];
                if (element.Guid == guid)
                {
                    return element;
                }
            }

            return null;
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
        private void ElementOnDestroyed(Element element)
        {
            _all.Remove(element);
        }
    }
}