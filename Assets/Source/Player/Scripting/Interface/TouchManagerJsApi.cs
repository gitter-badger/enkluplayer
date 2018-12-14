using CreateAR.Commons.Unity.Logging;
using CreateAR.EnkluPlayer.IUX;
using CreateAR.EnkluPlayer.Scripting;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// A JS API for registering for and responding to touch events on elements.
    /// </summary>
    [JsInterface("touch")]
    public class TouchManagerJsApi : ITouchDelegate
    {
        /// <summary>
        /// Event names.
        /// </summary>
        public const string EVENT_TOUCH_STARTED = "touchstarted";
        public const string EVENT_TOUCH_STOPPED = "touchstopped";
        public const string EVENT_TOUCH_DRAGGED = "touchdragged";

        /// <summary>
        /// Underlying touch system.
        /// </summary>
        private readonly ITouchManager _touch;
        private readonly IElementJsCache _cache;

        /// <summary>
        /// Constructor.
        /// </summary>
        public TouchManagerJsApi(
            ITouchManager touch,
            IElementJsCache cache)
        {
            _touch = touch;
            _cache = cache;
        }

        /// <see cref="ITouchManager"/>
        public bool register(ElementJs element)
        {
            // pass to TouchManager
            if (!_touch.Register(element.Element, this))
            {
                return false;
            }
            
            return true;
        }

        /// <see cref="ITouchManager"/>
        public bool unregister(ElementJs element)
        {
            // pass to TouchManager
            if (!_touch.Unregister(element.Element))
            {
                return false;
            }
            
            return true;
        }

        /// <inheritdoc cref="ITouchDelegate"/>
        public void TouchStarted(Element element, Vector3 intersection)
        {
            var elementJs = _cache.Element(element);
            if (null == elementJs)
            {
                Log.Warning(this, "Received a Touched message from unregistered Element {0}.", element);
                return;
            }

            elementJs.dispatch(EVENT_TOUCH_STARTED, intersection);
        }

        /// <inheritdoc cref="ITouchDelegate"/>
        public void TouchDragged(Element element, Vector3 intersection)
        {
            var elementJs = _cache.Element(element);
            if (null == elementJs)
            {
                Log.Warning(this, "Received a Touched message from unregistered Element {0}.", element);
                return;
            }

            elementJs.dispatch(EVENT_TOUCH_DRAGGED, intersection);
        }

        /// <inheritdoc cref="ITouchDelegate"/>
        public void TouchStopped(Element element)
        {
            var elementJs = _cache.Element(element);
            if (null == elementJs)
            {
                Log.Warning(this, "Received a Touched message from unregistered Element {0}.", element);
                return;
            }

            elementJs.dispatch(EVENT_TOUCH_STOPPED);
        }
    }
}