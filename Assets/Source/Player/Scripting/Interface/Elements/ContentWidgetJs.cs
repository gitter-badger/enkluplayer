using CreateAR.EnkluPlayer.IUX;
using Jint.Runtime;
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
            _contentWidget = (ContentWidget) _element;
            _contentWidget.OnLoaded.OnSuccess(CacheComponents);
        }

        /// <inheritdoc />
        public override void Cleanup()
        {
            base.Cleanup();

            _contentWidget.OnLoaded.Remove(CacheComponents);
            
            TeardownComponents();
        }

        /// <summary>
        /// Attempts to set <see cref="animator"/>.
        /// </summary>
        private void CacheComponents(ContentWidget contentWidget)
        {
            TeardownComponents();
            
            // Animator
            var unityAnimator = contentWidget.GetComponent<Animator>();
            if (unityAnimator != null) 
            {
                var anim = new UnityAnimator(unityAnimator);
                animator = new AnimatorJsApi(_element.Schema, anim);
                animator.Setup();
            }
            else
            {
                animator = null;
            }
            
            // Material
            var unityRenderer = contentWidget.GetComponent<Renderer>();
            if (unityRenderer != null && unityRenderer.sharedMaterial != null)
            {
                var renderer = new UnityRenderer(unityRenderer); 
                material = new MaterialJsApi(_element.Schema, renderer);
                material.Setup();
            }
            else
            {
                material = null;
            }

            // Audio
            var unityAudioSource = contentWidget.GetComponent<AudioSource>();
            if (unityAudioSource != null)
            {
                var audioSource = new UnityAudioSource(unityAudioSource);
                audio = new AudioJsApi(_element.Schema, audioSource);
                audio.Setup();
            }
            else
            {
                audio = null;
            }
        }

        /// <summary>
        /// Calls Teardown on any existing components.
        /// </summary>
        private void TeardownComponents()
        {
            if (animator != null)
            {
                animator.Teardown();
            }
            
            if (material != null)
            {
                material.Teardown();
            }
            
            if (audio != null)
            {
                audio.Teardown();
            }
        }
    }
}
