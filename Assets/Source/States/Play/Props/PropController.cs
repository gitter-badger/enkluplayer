using System;
using CreateAR.Commons.Unity.Async;
using CreateAR.SpirePlayer.IUX;
using UnityEngine;
using Void = CreateAR.Commons.Unity.Async.Void;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Associates a <c>ContentWidget</c> and a <c>PropData</c>.
    /// </summary>
    public class PropController : MonoBehaviour
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
        private IPropUpdateDelegate _delegate;

        /// <summary>
        /// Controls the prop splash menu.
        /// </summary>
        private PropSplashController _splashController;
        
        /// <summary>
        /// Time of last save.
        /// </summary>
        private DateTime _lastSave;

        /// <summary>
        /// Save token.
        /// </summary>
        private IAsyncToken<Void> _saveToken;

        /// <summary>
        /// True iff needs to save.
        /// </summary>
        private bool _isDirty;

        private ElementSchemaProp<Vec3> _positionProp;
        private ElementSchemaProp<Vec3> _rotationProp;
        private ElementSchemaProp<Vec3> _scaleProp;

        /// <summary>
        /// The Element.
        /// </summary>
        public Element Element { get; private set; }

        /// <summary>
        /// Called when prop adjust is requested.
        /// </summary>
        public event Action<PropController> OnAdjust;
        
        /// <summary>
        /// Initializes the controller. Updates are sent through the delegate.
        /// </summary>
        /// <param name="element">The elementto watch.</param>
        /// <param name="delegate">The delegate to push events through.</param>
        public void Initialize(
            Element element,
            IPropUpdateDelegate @delegate)
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
        /// Creates splash menu.
        /// </summary>
        private void InitializeSplashMenu()
        {
            _splashController = gameObject.AddComponent<PropSplashController>();
            _splashController.OnOpen += Splash_OnOpen;
            _splashController.Initialize(Element.Schema.Get<string>("name").Value);
        }
        
        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            var trans = gameObject.transform;

            // check for position changes
            {
                if (!trans.position.Approximately(
                    _positionProp.Value.ToVector(),
                    POSITION_EPSILON))
                {
                    _positionProp.Value = trans.position.ToVec();

                    _isDirty = true;
                }
            }

            // check for rotation changes
            {
                if (!trans.rotation.eulerAngles.Approximately(
                    _rotationProp.Value.ToVector(),
                    ROTATION_EPSILON))
                {
                    _rotationProp.Value = trans.rotation.eulerAngles.ToVec();

                    _isDirty = true;
                }
            }

            // check for scale changes
            {
                if (!trans.localScale.Approximately(
                    _scaleProp.Value.ToVector(),
                    SCALE_EPSILON))
                {
                    _scaleProp.Value = trans.localScale.ToVec();

                    _isDirty = true;
                }
            }

            var now = DateTime.Now;
            if (_isDirty
                && null == _saveToken
                && now.Subtract(_lastSave).TotalSeconds > TIME_EPSILON)
            {
                _isDirty = false;
                _lastSave = now;

                _saveToken = _delegate.Update(Element);
                _saveToken.OnFinally(_ => _saveToken = null);
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