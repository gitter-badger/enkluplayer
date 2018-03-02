using System;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Design mode controller for a ContentWidget.
    /// </summary>
    public class ContentDesignController : ElementDesignController
    {
        /// <summary>
        /// Context for this type of controller.
        /// </summary>
        public class ContentDesignControllerContext
        {
            /// <summary>
            /// Call when adjust is requested.
            /// </summary>
            public Action<ContentDesignController> OnAdjust;

            /// <summary>
            /// The delegate to push updates to.
            /// </summary>
            public IElementUpdateDelegate Delegate;
        }

        /// <summary>
        /// Constants.
        /// </summary>
        private const float EPSILON = 0.05f;
        private const float TIME_EPSILON = 0.1f;

        /// <summary>
        /// The context passed in.
        /// </summary>
        private ContentDesignControllerContext _context;

        /// <summary>
        /// Controls the prop splash menu.
        /// </summary>
        private ElementSplashController _splashController;
        
        /// <summary>
        /// Time of last finalize.
        /// </summary>
        private DateTime _lastFinalize;
        
        /// <summary>
        /// True iff needs to save.
        /// </summary>
        private bool _isDirty;

        /// <summary>
        /// True iff updates should be pushed to Schema.
        /// </summary>
        private bool _updatesEnabled = true;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<Vec3> _positionProp;
        private ElementSchemaProp<Vec3> _rotationProp;
        private ElementSchemaProp<Vec3> _scaleProp;
        
        /// <inheritdoc />
        public override void Initialize(Element element, object context)
        {
            base.Initialize(element, context);

            _context = (ContentDesignControllerContext) context;
            
            _positionProp = Element.Schema.Get<Vec3>("position");
            _rotationProp = Element.Schema.Get<Vec3>("rotation");
            _scaleProp = Element.Schema.Get<Vec3>("scale");

            InitializeSplashMenu();
        }
        
        /// <summary>
        /// Hides the splash menu.
        /// </summary>
        public void HideSplashMenu()
        {
            _splashController.Root.Schema.Set("visible", false);
        }

        /// <summary>
        /// Shows the splash menu.
        /// </summary>
        public void ShowSplashMenu()
        {
            _splashController.Root.Schema.Set("visible", true);
        }

        /// <summary>
        /// Disables pushing updates to schema.
        /// </summary>
        public void DisableUpdates()
        {
            _updatesEnabled = false;
        }

        /// <summary>
        /// Enables pushing updates to schema.
        /// </summary>
        public void EnableUpdates()
        {
            _updatesEnabled = true;
        }

        /// <summary>
        /// Pushes a final, exact state.
        /// </summary>
        public void FinalizeState()
        {
            UpdateDelegate(float.Epsilon);
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
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            if (!_updatesEnabled)
            {
                return;
            }

            var now = DateTime.Now;
            var isUpdateable = now.Subtract(_lastFinalize).TotalSeconds > TIME_EPSILON;

            if (isUpdateable)
            {
                UpdateDelegate(EPSILON);
            }
        }

        /// <summary>
        /// Pushes an update through the delegate.
        /// </summary>
        private void UpdateDelegate(float epsilon)
        {
            var trans = gameObject.transform;

            // check for position changes
            {
                if (!trans.position.Approximately(
                    _positionProp.Value.ToVector(),
                    epsilon))
                {
                    _positionProp.Value = trans.position.ToVec();

                    _context.Delegate.Update(Element, "position", _positionProp.Value);

                    _isDirty = true;
                }
            }

            // check for rotation changes
            {
                if (!trans.rotation.eulerAngles.Approximately(
                    _rotationProp.Value.ToVector(),
                    epsilon))
                {
                    _rotationProp.Value = trans.rotation.eulerAngles.ToVec();

                    _context.Delegate.Update(Element, "rotation", _rotationProp.Value);

                    _isDirty = true;
                }
            }

            // check for scale changes
            {
                if (!trans.localScale.Approximately(
                    _scaleProp.Value.ToVector(),
                    epsilon))
                {
                    _scaleProp.Value = trans.localScale.ToVec();

                    _context.Delegate.Update(Element, "scale", _scaleProp.Value);

                    _isDirty = true;
                }
            }

            if (_isDirty)
            {
                _isDirty = false;
                _lastFinalize = DateTime.Now;

                _context.Delegate.Finalize(Element);
            }
        }

        /// <summary>
        /// Called when the splash requests to open.
        /// </summary>
        private void Splash_OnOpen()
        {
            _context.OnAdjust(this);
        }
    }
}