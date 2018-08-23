using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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
        /// Backing variable for All property.
        /// </summary>
        private readonly ReadOnlyCollection<Element> _externalAll;

        /// <inheritdoc />
        public ReadOnlyCollection<Element> All
        {
            get { return _externalAll; }
        }

        /// <inheritdoc />
        public Action<Element> OnCreated { get; private set; }
        
        /// <inheritdoc />
        public Action<Element> OnDestroyed { get; private set; }

        /// <summary>
        /// Constructor.
        /// </summary>
        public ElementManager()
        {
            _externalAll = new ReadOnlyCollection<Element>(_all);
        }

        /// <inheritdoc />
        public void Add(Element element)
        {
            if (_all.Contains(element))
            {
                return;
            }
            
            element.OnDestroyed += ElementOnDestroyed;

            _all.Add(element);
            OnCreated(element);
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        public Element ById(string id)
        {
            for (int i = 0, len = _all.Count; i < len; i++)
            {
                var element = _all[i];
                if (element.Id == id)
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
            OnDestroyed(element);
        }
    }
}