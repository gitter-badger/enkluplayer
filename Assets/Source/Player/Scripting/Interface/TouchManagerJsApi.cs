using System.Collections.Generic;
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

        /// <summary>
        /// Registered JS elements.
        ///
        /// TODO: Use the script cache instead of managing a separate list that
        /// TODO: can get out of sync.
        /// </summary>
        private readonly List<ElementJs> _registered = new List<ElementJs>();

        /// <summary>
        /// Constructor.
        /// </summary>
        public TouchManagerJsApi(ITouchManager touch)
        {
            _touch = touch;
        }

        public bool register(ElementJs element)
        {
            // pass to TouchManager
            if (!_touch.Register(element.Element, this))
            {
                return false;
            }

            // TODO: listen for destroy so we can unregister

            // add
            _registered.Add(element);

            return true;
        }

        public bool unregister(ElementJs element)
        {
            // pass to TouchManager
            if (!_touch.Unregister(element.Element))
            {
                return false;
            }

            _registered.Remove(element);
            return true;
        }

        public void TouchStarted(Element element, Vector3 intersection)
        {
            var elementJs = ElementJs(element);
            if (null == elementJs)
            {
                Log.Warning(this, "Received a Touched message from unregistered Element {0}.", element);
                return;
            }

            elementJs.dispatch(EVENT_TOUCH_STARTED, intersection);
        }

        public void TouchDragged(Element element, Vector3 intersection)
        {
            var elementJs = ElementJs(element);
            if (null == elementJs)
            {
                Log.Warning(this, "Received a Touched message from unregistered Element {0}.", element);
                return;
            }

            elementJs.dispatch(EVENT_TOUCH_DRAGGED, intersection);
        }

        public void TouchStopped(Element element)
        {
            var elementJs = ElementJs(element);
            if (null == elementJs)
            {
                Log.Warning(this, "Received a Touched message from unregistered Element {0}.", element);
                return;
            }

            elementJs.dispatch(EVENT_TOUCH_STOPPED);
        }

        private ElementJs ElementJs(Element element)
        {
            for (int i = 0, len = _registered.Count; i < len; i++)
            {
                var elementJs = _registered[i];
                if (elementJs.Element == element)
                {
                    return elementJs;
                }
            }

            return null;
        }
    }
}