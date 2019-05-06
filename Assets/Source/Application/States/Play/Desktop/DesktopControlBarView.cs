using System;
using System.Collections.Generic;
using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using RLD;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.EnkluPlayer
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
        /// Fullscreen button.
        /// </summary>
        public OnPointerDownButton FullScreen;

        /// <summary>
        /// Messages.
        /// </summary>
        [Inject]
        public IMessageRouter Messages { get; set; }

        /// <summary>
        /// Transform space toggle.
        /// </summary>
        public Toggle TransformSpace;

        /// <summary>
        /// Called when the translate button has been pressed.
        /// </summary>
        public void OnTranslate()
        {
            RTObjectSelectionGizmos.Get.SetWorkGizmo(ObjectSelectionGizmoId.MoveGizmo);
        }

        /// <summary>
        /// Called when the rotate button has been pressed.
        /// </summary>
        public void OnRotate()
        {
            RTObjectSelectionGizmos.Get.SetWorkGizmo(ObjectSelectionGizmoId.RotationGizmo);
        }

        /// <summary>
        /// Called when the scale button has been pressed.
        /// </summary>
        public void OnScale()
        {
            RTObjectSelectionGizmos.Get.SetWorkGizmo(ObjectSelectionGizmoId.ScaleGizmo);
        }

        /// <summary>
        /// Called when the value of the transform space checkbox is changed.
        /// </summary>
        public void OnValueChanged()
        {
            var isChecked = TransformSpace.isOn;

            Log.Info(this, "OnValueChanged({0})", isChecked);

            RTObjectSelectionGizmos.Get.SetTransformSpace(isChecked ? GizmoSpace.Global : GizmoSpace.Local);
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
            _unsubs.Add(Messages.Subscribe(MessageTypes.BRIDGE_HELPER_TRANSFORM_SPACE, Messages_OnTransformSpace));

            RTObjectSelectionGizmos.Get.OnSpaceUpdated += space => TransformSpace.isOn = space == GizmoSpace.Global;
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void OnDestroy()
        {
            // unsubscribe
            for (int i = 0, len = _unsubs.Count; i < len; i++)
            {
                _unsubs[i]();
            }
            _unsubs.Clear();
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Update()
        {
            if (Input.GetKeyUp(KeyCode.F))
            {
                // focus
                RTFocusCamera.Get.Focus(RTObjectSelection.Get.GetWorldAABB());
            }
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

        /// <summary>
        /// Called when transform space change is requested.
        /// </summary>
        /// <param name="evt">The event.</param>
        private void Messages_OnTransformSpace(object evt)
        {
            TransformSpace.isOn = !TransformSpace.isOn;

            OnValueChanged();
        }
    }
}