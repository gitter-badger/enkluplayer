using System.Collections.Generic;
using System.Diagnostics;
using CreateAR.Commons.Unity.Logging;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Simple IGizmoManager implementation.
    /// </summary>
    public class GizmoManager : InjectableMonoBehaviour, IGizmoManager
    {
        /// <summary>
        /// List of all gizmo renderers.
        /// </summary>
        private readonly List<IGizmoRenderer> _all = new List<IGizmoRenderer>();

        /// <summary>
        /// Backing variable for property.
        /// </summary>
        private bool _isVisible = true;

        /// <summary>
        /// MonoBehaviours.
        /// </summary>
        public MonoBehaviourGizmoRenderer QrAnchor;
        public MonoBehaviourGizmoRenderer Light;
        public MonoBehaviourGizmoRenderer WorldAnchor;

        /// <summary>
        /// Stands in for editor.
        /// </summary>
        [Inject]
        public EditorSettings Editor { get; set; }

        /// <summary>
        /// True iff gizmos should be visible.
        /// </summary>
        public bool IsVisible
        {
            get
            {
                return _isVisible;
            }
            set
            {
                _isVisible = value;

                for (int i = 0, len = _all.Count; i < len; i++)
                {
                    _all[i].IsVisible = _isVisible;
                }
            }
        }

        [PostConstruct]
//        public void CheckMode()
//        {
//            IsVisible = Editor.Settings.ElementGizmos;
//        }

        /// <inheritdoc />
        public void Track(Element element)
        {
            Verbose("Track({0})", element.GetType().Name);

            var impl = Renderer(element);
            if (null != impl)
            {
                _all.Add(impl);

                impl.Initialize(element);
                impl.IsVisible = IsVisible;

                element.OnDestroyed += _ =>
                {
                    impl.Uninitialize();

                    _all.Remove(impl);
                };
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