using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Simple IGizmoManager implementation.
    /// </summary>
    public class GizmoManager : InjectableMonoBehaviour, IGizmoManager
    {
        /// <summary>
        /// MonoBehaviours.
        /// </summary>
        public MonoBehaviourGizmoRenderer QrAnchor;
        public MonoBehaviourGizmoRenderer Light;
        public MonoBehaviourGizmoRenderer WorldAnchor;

        /// <inheritdoc />
        public void Track(Element element)
        {
            Verbose("Track({0})", element.GetType().Name);

            var impl = Renderer(element);
            if (null != impl)
            {
                impl.Initialize(element);

                element.OnDestroyed += _ => impl.Uninitialize();
            }
        }

        /// <summary>
        /// Retrieves a renderer for an element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <returns></returns>
        private IGizmoRenderer Renderer(Element element)
        {
            var type = element.GetType();
            if (typeof(QrAnchorWidget) == type)
            {
                return Instantiate(QrAnchor);
            }

            if (typeof(LightWidget) == type)
            {
                return Instantiate(Light);
            }

            if (typeof(WorldAnchorWidget) == type)
            {
                return Instantiate(WorldAnchor);
            }

            return null;
        }

        /// <summary>
        /// Verbose logging.
        /// </summary>
        [Conditional("LOGGING_VERBOSE")]
        private void Verbose(string message, params object[] replacements)
        {
            Log.Info(this, message, replacements);
        }
    }
}