using System;
using CreateAR.Commons.Unity.Logging;
using CreateAR.SpirePlayer.IUX;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Controls splash menus.
    /// </summary>
    public class ElementSplashDesignController : ElementUpdateDesignController
    {
        /// <summary>
        /// Subclass with adjust delegate.
        /// </summary>
        public class DesignContext : Context
        {
            /// <summary>
            /// Call when adjust is requested.
            /// </summary>
            public Action<ElementSplashDesignController> OnAdjust;
        }

        /// <summary>
        /// Controls the prop splash menu.
        /// </summary>
        private ElementSplashController _splashController;
        
        /// <inheritdoc />
        public override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);
            
            _splashController.OnOpen += Splash_OnOpen;
            _splashController.Initialize(Element);

            ShowSplashMenu();

            Log.Info(this, "Initialize({0})", element.Id);
        }

        /// <inheritdoc />
        public override void Uninitialize()
        {
            Log.Info(this, "Uninitialize({0})", Element.Id);

            base.Uninitialize();

            _splashController.OnOpen -= Splash_OnOpen;

            HideSplashMenu();
        }

        /// <summary>
        /// Hides the splash menu.
        /// </summary>
        public void HideSplashMenu()
        {
            if (null != _splashController)
            {
                _splashController.Root.Schema.Set("visible", false);
            }
        }

        /// <summary>
        /// Shows the splash menu.
        /// </summary>
        public void ShowSplashMenu()
        {
            if (null != _splashController)
            {
                _splashController.Root.Schema.Set("visible", true);
            }
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Awake()
        {
            _splashController = gameObject.AddComponent<ElementSplashController>();

            HideSplashMenu();
        }

        /// <summary>
        /// Called when the splash requests to open.
        /// </summary>
        private void Splash_OnOpen()
        {
            ((DesignContext) _context).OnAdjust(this);
        }
    }
}