using System;
using CreateAR.SpirePlayer.IUX;
using Jint;
using UnityEngine;

namespace CreateAR.SpirePlayer.Scripting
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
        /// Constructor.
        /// </summary>
        /// <param name="scripts"></param>
        /// <param name="cache"></param>
        /// <param name="engine"></param>
        /// <param name="element"></param>
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
            try
            {
                var unityAnimator = contentWidget.GetComponent<Animator>();
                if (unityAnimator != null) 
                {
                    animator = new AnimatorJsApi(unityAnimator);
                }
            }
            catch (NullReferenceException) 
            {
                // Set to null if ContentWidget has no Asset attached to it.
                animator = null;
            }
        }
    }
}
