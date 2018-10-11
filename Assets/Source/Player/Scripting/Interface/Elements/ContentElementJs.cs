using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer.Scripting
{
    /// <summary>
    /// ElementJs derivation for Elements that are ContentWidgets.
    /// </summary>
    public class ContentElementJs : ElementJs
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
        /// Constructor.
        /// </summary>
        public ContentElementJs(
            IScriptManager scripts,
            IElementJsCache cache,
            Element element) 
            : base(scripts, cache, element)
        {
            ((ContentWidget) _element).OnLoaded.OnSuccess(CacheAnimator);
        }

        /// <inheritdoc />
        public override void Cleanup()
        {
            base.Cleanup();

            ((ContentWidget) _element).OnLoaded.Remove(CacheAnimator);
        }

        /// <summary>
        /// Attempts to set <see cref="animator"/>.
        /// </summary>
        private void CacheAnimator(ContentWidget contentWidget)
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
