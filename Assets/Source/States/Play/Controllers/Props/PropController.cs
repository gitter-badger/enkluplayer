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
            trans.localScale = data.Scale.ToVector();

            Data = data;
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
                    Data.Scale.ToVector(),
                    SCALE_EPSILON))
                {
                    Data.Scale = trans.localScale.ToVec();

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
    }
}