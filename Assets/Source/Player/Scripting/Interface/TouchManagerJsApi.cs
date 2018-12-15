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
        /// Info passed to js.
        /// </summary>
        public class HitInfoJs
        {
            /// <summary>
            /// The position of the intersection.
            /// </summary>
            public Vector3 position;

            /// <summary>
            /// A vector normal to the surface at the point of intersection.
            /// </summary>
            public Vector3 normal;
        }

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

        /// <see cref="ITouchManager"/>
        public Vec2 fingerOffset
        {
            get { return _touch.FingerOffset; }
            set { _touch.FingerOffset = value; }
        }
        
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
        [DenyJsAccess]
        public void TouchStarted(Element element, Vector3 intersection, Vector3 surfaceNormal)
        {
            var elementJs = _cache.Element(element);
            if (null == elementJs)
            {
                Log.Warning(this, "Received a Touched message from unregistered Element {0}.", element);
                return;
            }

            elementJs.dispatch(EVENT_TOUCH_STARTED, new HitInfoJs
            {
                position = intersection,
                normal = surfaceNormal
            });
        }

        /// <inheritdoc cref="ITouchDelegate"/>
        [DenyJsAccess]
        public void TouchDragged(Element element, Vector3 intersection, Vector3 surfaceNormal)
        {
            var elementJs = _cache.Element(element);
            if (null == elementJs)
            {
                Log.Warning(this, "Received a Touched message from unregistered Element {0}.", element);
                return;
            }

            elementJs.dispatch(EVENT_TOUCH_DRAGGED, new HitInfoJs
            {
                position = intersection,
                normal = surfaceNormal
            });
        }

        /// <inheritdoc cref="ITouchDelegate"/>
        [DenyJsAccess]
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