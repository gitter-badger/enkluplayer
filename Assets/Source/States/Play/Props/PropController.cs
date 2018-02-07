using System;
using CreateAR.Commons.Unity.Async;
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

        /// <summary>
        /// The PropData.
        /// </summary>
        public PropData Data { get; private set; }

        /// <summary>
        /// The ContentWidget.
        /// </summary>
        public ContentWidget Content { get; private set; }

        /// <summary>
        /// Called when prop adjust is requested.
        /// </summary>
        public event Action<PropController> OnAdjust;
        
        /// <summary>
        /// Initializes the controller. Updates are sent through the delegate.
        /// </summary>
        /// <param name="data">The data to edit.</param>
        /// <param name="content">The content to watch.</param>
        /// <param name="delegate">The delegate to push events through.</param>
        public void Initialize(
            PropData data,
            ContentWidget content,
            IPropUpdateDelegate @delegate)
        {
            Data = data;
            Content = content;
            
            _delegate = @delegate;

            InitializeSplashMenu();

            Resync(Data);
        }

        /// <summary>
        /// Stops the controller from updating data anymore.
        /// </summary>
        public void Uninitialize()
        {
            Data = null;
            Content = null;

            _delegate = null;
        }

        /// <summary>
        /// Forcibly resyncs. Should only be called between Initialize and Uninitialize.
        /// </summary>
        /// <param name="data">The PropData to sync with.</param>
        public void Resync(PropData data)
        {
            var trans = Content.GameObject.transform;

            trans.position = data.Position.ToVector();
            trans.localRotation = Quaternion.Euler(data.Rotation.ToVector());
            trans.localScale = data.LocalScale.ToVector();

            Data = data;
        }

        public void HideSplashMenu()
        {
            _splashController.Root.Schema.Set("visible", false);
        }

        public void ShowSplashMenu()
        {
            _splashController.Root.Schema.Set("visible", true);
        }

        /// <summary>
        /// Creates splash menu.
        /// </summary>
        private void InitializeSplashMenu()
        {
            _splashController = Content.GameObject.AddComponent<PropSplashController>();
            _splashController.OnOpen += Splash_OnOpen;
            _splashController.Initialize(Data);
        }

        /// <inheritdoc cref="MonoBehaviour"/>
        private void Update()
        {
            if (null == Data)
            {
                return;
            }

            var trans = Content.GameObject.transform;

            // check for position changes
            {
                if (!trans.position.Approximately(
                    Data.Position.ToVector(),
                    POSITION_EPSILON))
                {
                    Data.Position = trans.position.ToVec();

                    _isDirty = true;
                }
            }

            // check for rotation changes
            {
                if (!trans.rotation.eulerAngles.Approximately(
                    Data.Rotation.ToVector(),
                    ROTATION_EPSILON))
                {
                    Data.Rotation = trans.rotation.eulerAngles.ToVec();

                    _isDirty = true;
                }
            }

            // check for scale changes
            {
                if (!trans.localScale.Approximately(
                    Data.LocalScale.ToVector(),
                    SCALE_EPSILON))
                {
                    Data.LocalScale = trans.localScale.ToVec();

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

                _saveToken = _delegate.Update(Data);
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