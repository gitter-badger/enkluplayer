using System;
using CreateAR.EnkluPlayer.IUX;
using UnityEngine;

namespace CreateAR.EnkluPlayer
{
    /// <summary>
    /// Manages the splash menu.
    /// </summary>
    [InjectVine("Element.Select")]
    public class ElementSelectionMenuController : InjectableIUXController
    {
        /// <summary>
        /// Button color prop.
        /// </summary>
        private ElementSchemaProp<string> _colorProp;

        /// <summary>
        /// Previous value.
        /// </summary>
        private string _colorInitialValue;

        /// <summary>
        /// True iff MarkAsTarget has been called. OnDisable will reset this.
        /// </summary>
        public bool IsTarget { get; private set; }

        /// <summary>
        /// Button button.
        /// </summary>
        public ButtonWidget Btn
        {
            get { return (ButtonWidget) Root; }
        }
        
        /// <summary>
        /// Called when selected.
        /// </summary>
        public event Action OnSelected;

        /// <summary>
        /// Marks as the target. This is reset on disable.
        /// </summary>
        public void MarkAsTarget()
        {
            _colorProp.Value = "Negative";

            IsTarget = true;
        }
        
        /// <inheritdoc />
        protected override void Awake()
        {
            base.Awake();

            Btn.Activator.OnActivated += Btn_OnActivated;

            _colorProp = Btn.Schema.Get<string>("ready.color");
            _colorInitialValue = _colorProp.Value;
        }

        /// <inheritdoc />
        protected override void OnDisable()
        {
            base.OnDisable();

            _colorProp.Value = _colorInitialValue;
            IsTarget = false;
        }

        /// <inheritdoc cref="MonoBehaviour" />
        private void Update()
        {
            // scale
            var scale = transform.lossyScale;
            var adjustedScale = new Vector3(
                1f / scale.x,
                1f / scale.y,
                1f / scale.z);
            Btn.Activator.GameObject.transform.localScale = adjustedScale;
        }

        /// <summary>
        /// Called when the button has been activated.
        /// </summary>
        /// <param name="activatorPrimitive">Activator primitive.</param>
        private void Btn_OnActivated(ActivatorPrimitive activatorPrimitive)
        {
            if (null != OnSelected)
            {
                OnSelected();
            }
        }
    }
}