using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// ElementJs derivation for Elements that are ContentWidgets.
    /// </summary>
    public class ContentWidgetJs : ElementJs
    {
        /// <summary>
        /// The animator interface.
        /// </summary>
        public AnimatorJsApi animator { get; private set; }

        /// <summary>
        /// The material interface.
        /// </summary>
        public MaterialJsApi material { get; private set; }

        /// <summary>
        /// The audio interface.
        /// </summary>
        public AudioJsApi audio { get; private set; }

        /// <summary>
        /// The underling ContentWidget.
        /// </summary>
        [DenyJsAccess]
        public ContentWidget ContentWidget
        {
            get { return _contentWidget; }
        }
        
        /// <summary>
        /// Backing variable.
        /// </summary>
        private readonly ContentWidget _contentWidget;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ContentWidgetJs(
            IScriptManager scripts,
            IElementJsCache cache,
            Element element) 
            : base(scripts, cache, element)
        {
            _contentWidget = ((ContentWidget) _element);
            _contentWidget.OnLoaded.OnSuccess(CacheComponents);
        }

        /// <inheritdoc />
        public override void Cleanup()
        {
            base.Cleanup();

            ((ContentWidget) _element).OnLoaded.Remove(CacheComponents);
        }

        /// <summary>
        /// Attempts to set <see cref="animator"/>.
        /// </summary>
        private void CacheComponents(ContentWidget contentWidget)
        {
            var unityAnimator = contentWidget.GetComponent<Animator>();
            if (unityAnimator != null) 
            {
                animator = new AnimatorJsApi(unityAnimator);
            }
            else
            {
                animator = null;
            }

            var unityRenderer = contentWidget.GetComponent<Renderer>();
            if (unityRenderer != null && unityRenderer.sharedMaterial != null)
            {
                material = new MaterialJsApi(unityRenderer);
            }
            else
            {
                material = null;
            }

            var unityAudioSource = contentWidget.GetComponent<AudioSource>();
            if (unityAudioSource != null)
            {
                audio = new AudioJsApi(unityAudioSource);
            }
            else
            {
                audio = null;
            }
        }
    }
}
