using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Messaging;
using RTEditor;
using UnityEngine;

namespace CreateAR.SpirePlayer
{
    /// <summary>
    /// Responds to control bar.
    /// </summary>
    public class DesktopControlBarView : InjectableMonoBehaviour
    {
        /// <summary>
        /// List of messaging unsubscribes.
        /// </summary>
        private readonly List<Action> _unsubs = new List<Action>();

        /// <summary>
        /// The system to change.
        /// </summary>
        public EditorGizmoSystem Gizmos;

        /// <summary>
        /// Fullscreen button.
        /// </summary>
        public OnPointerDownButton FullScreen;

        /// <summary>
        /// Messages.
        /// </summary>
        [Inject]
        public IMessageRouter Messages { get; set; }
        
        /// <summary>
        /// Called when the translate button has been pressed.
        /// </summary>
        public void OnTranslate()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Translation);
        }

        /// <summary>
        /// Called when the rotate button has been pressed.
        /// </summary>
        public void OnRotate()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Rotation);
        }

        /// <summary>
        /// Called when the scale button has been pressed.
        /// </summary>
        public void OnScale()
        {
            Gizmos.ChangeActiveGizmo(GizmoType.Scale);
        }

        /// <inheritdoc cref="MonoBehaviour" />
        protected override void Awake()
        {
            base.Awake();

            FullScreen.OnClicked += () =>
            {
                Screen.fullScreen = !Screen.fullScreen;
                Cursor.lockState = Cursor.lockState == CursorLockMode.Confined
                    ? CursorLockMode.None
                    : CursorLockMode.Confined;
            };

            // watch for gizmo events
            _unsubs.Add(Messages.Subscribe(MessageTypes.BRIDGE_HELPER_GIZMO_TRANSLATION, Messages_OnTranslation));
            _unsubs.Add(Messages.Subscribe(MessageTypes.BRIDGE_HELPER_GIZMO_ROTATION, Messages_OnRotation));
            _unsubs.Add(Messages.Subscribe(MessageTypes.BRIDGE_HELPER_GIZMO_SCALE, Messages_OnScale));
        }

        private void OnDestroy()
        {
            // unsubscribe
            for (int i = 0, len = _unsubs.Count; i < len; i++)
            {
                _unsubs[i]();
            }
            _unsubs.Clear();
        }

        /// <summary>
        /// Called when translation is requested.
        /// </summary>
        /// <param name="evt">The event.</param>
        private void Messages_OnTranslation(object @evt)
        {
            OnTranslate();
        }

        /// <summary>
        /// Called when rotation is requested.
        /// </summary>
        /// <param name="evt">The event.</param>
        private void Messages_OnRotation(object @evt)
        {
            OnRotate();
        }

        /// <summary>
        /// Called when scale is requested.
        /// </summary>
        /// <param name="evt">The event.</param>
        private void Messages_OnScale(object @evt)
        {
            OnScale();
        }
    }
}