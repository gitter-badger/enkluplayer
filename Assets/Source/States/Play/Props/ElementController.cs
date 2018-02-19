using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Pushes Element updates to a delegate.
    /// </summary>
    public class ElementController : MonoBehaviour
    {
        /// <summary>
        /// Constants.
        /// </summary>
        private const float POSITION_EPSILON = 0.1f;
        private const float ROTATION_EPSILON = 0.1f;
        private const float SCALE_EPSILON = 0.1f;
        private const float TIME_EPSILON = 0.5f;
        
        /// <summary>
        /// The delegate to push updates through.
        /// </summary>
        private IElementUpdateDelegate _delegate;

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

        /// <summary>
        /// The Element.
        /// </summary>
        public Element Element { get; private set; }

        /// <summary>
        /// Called when adjust is requested.
        /// </summary>
        public event Action<ElementController> OnAdjust;
        
        /// <summary>
        /// Initializes the controller. Updates are sent through the delegate.
        /// </summary>
        /// <param name="element">The elementto watch.</param>
        /// <param name="delegate">The delegate to push events through.</param>
        public void Initialize(
            Element element,
            IElementUpdateDelegate @delegate)
        {
            Element = element;
            _delegate = @delegate;

            _positionProp = Element.Schema.Get<Vec3>("position");
            _rotationProp = Element.Schema.Get<Vec3>("rotation");
            _scaleProp = Element.Schema.Get<Vec3>("scale");

            InitializeSplashMenu();
        }

        /// <summary>
        /// Stops the controller from updating data anymore.
        /// </summary>
        public void Uninitialize()
        {
            Element = null;

            _delegate = null;
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

            var trans = gameObject.transform;
            var now = DateTime.Now;
            var isUpdateable = now.Subtract(_lastFinalize).TotalSeconds > TIME_EPSILON;

            // check for position changes
            {
                if (isUpdateable
                    && !trans.position.Approximately(
                        _positionProp.Value.ToVector(),
                        POSITION_EPSILON))
                {
                    _positionProp.Value = trans.position.ToVec();

                    _delegate.Update(Element, "position", _positionProp.Value);

                    _isDirty = true;
                }
            }

            // check for rotation changes
            {
                if (isUpdateable
                    && !trans.rotation.eulerAngles.Approximately(
                        _rotationProp.Value.ToVector(),
                        ROTATION_EPSILON))
                {
                    _rotationProp.Value = trans.rotation.eulerAngles.ToVec();

                    _delegate.Update(Element, "rotation", _rotationProp.Value);

                    _isDirty = true;
                }
            }

            // check for scale changes
            {
                if (isUpdateable
                    && !trans.localScale.Approximately(
                        _scaleProp.Value.ToVector(),
                        SCALE_EPSILON))
                {
                    _scaleProp.Value = trans.localScale.ToVec();

                    _delegate.Update(Element, "scale", _scaleProp.Value);

                    _isDirty = true;
                }
            }

            if (_isDirty)
            {
                _isDirty = false;
                _lastFinalize = now;

                _delegate.Finalize(Element);
            }
        }
        
        /// <summary>
        /// Called when the splash requests to open.
        /// </summary>
        private void Splash_OnOpen()
        {
            if (null != OnAdjust)
            {
                OnAdjust(this);
            }
        }
    }
}