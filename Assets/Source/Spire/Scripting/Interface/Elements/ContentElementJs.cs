using CreateAR.SpirePlayer.IUX;
using Jint;
using UnityEngine;

namespace CreateAR.SpirePlayer.Scripting
{
    public class ContentElementJs : ElementJs
    {
        /// <summary>
        /// The animator interface.
        /// </summary>
        public AnimatorJsApi animator { get; private set; }

        private readonly ContentWidget _contentWidget;

        public ContentElementJs(
            IScriptManager scripts, 
            IElementJsCache cache, 
            Engine engine, 
            Element element) 
            : base(scripts, cache, engine, element)
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
        }
    }
}
