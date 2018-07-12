using System;
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

            InitializeSplashMenu();
        }

        /// <inheritdoc />
        public override void Uninitialize()
        {
            base.Uninitialize();

            UninitializeSplashController();
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

        /// <summary>
        /// Creates splash menu.
        /// </summary>
        private void InitializeSplashMenu()
        {
            _splashController = gameObject.AddComponent<ElementSplashController>();
            _splashController.OnOpen += Splash_OnOpen;
            _splashController.Initialize(Element.Schema.Get<string>("name").Value);
        }

        /// <summary>
        /// Uninits the splash controller.
        /// </summary>
        private void UninitializeSplashController()
        {
            if (null != _splashController)
            {
                _splashController.OnOpen -= Splash_OnOpen;

                Destroy(_splashController);
                _splashController = null;
            }
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