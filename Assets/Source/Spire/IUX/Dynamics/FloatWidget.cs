using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Layout element.
    /// </summary>
    public class FloatWidget : Widget
    {
        /// <summary>
        /// Configuration.
        /// </summary>
        private readonly WidgetConfig _config;

        /// <summary>
        /// Dependencies.
        /// </summary>
        private readonly IIntentionManager _intention;

        /// <summary>
        /// Renders the float.
        /// </summary>
        private FloatRenderer _renderer;

        /// <summary>
        /// Props.
        /// </summary>
        private ElementSchemaProp<Vec3> _positionProp;
        private ElementSchemaProp<Vec3> _focusProp;
        private ElementSchemaProp<float> _reorientProp;

        /// <summary>
        /// Constructor.
        /// </summary>
        public FloatWidget(
            GameObject gameObject,
            WidgetConfig config,
            IIntentionManager intention,
            IMessageRouter messages,
            ILayerManager layers,
            TweenConfig tweens,
            ColorConfig colors)
            : base(
                gameObject,
                config,
                layers,
                tweens,
                colors,
                messages)
        {
            _config = config;
            _intention = intention;
        }

        /// <inheritdoc cref="Element"/>
        protected override void BeforeLoadChildrenInternal()
        {
            base.BeforeLoadChildrenInternal();

            // children need to be added to this
            _renderer = Object.Instantiate(
                _config.Float,
                Vector3.zero,
                Quaternion.identity);
            _renderer.transform.SetParent(GameObject.transform, false);
            _renderer.Initialize(this, _intention);
        }

        /// <inheritdoc cref="Element"/>
        protected override void AfterLoadChildrenInternal()
        {
            base.AfterLoadChildrenInternal();

            // props
            {
                _positionProp = Schema.GetOwn("position", new Vec3(0, 0, 2.5f));
                _positionProp.OnChanged += Position_OnChanged;
                UpdateRendererPosition();
                
                _focusProp = Schema.Get<Vec3>("focus");
                _focusProp.OnChanged += Focus_OnChanged;
                UpdateFocus();

                _reorientProp = Schema.Get<float>("fov.reorient");
                _reorientProp.OnChanged += Reorient_OnChanged;
                UpdateReorient();
            }
        }

        /// <inheritdoc cref="Element"/>
        protected override void AfterUnloadChildrenInternal()
        {
            _positionProp.OnChanged -= Position_OnChanged;
            _focusProp.OnChanged -= Focus_OnChanged;

            Object.Destroy(_renderer.gameObject);

            base.AfterUnloadChildrenInternal();
        }
        
        /// <inheritdoc />
        protected override Transform GetChildHierarchyParent(Widget child)
        {
            return _renderer.StationaryWidget.transform;
        }

        /// <summary>
        /// Updates the renderer's position.
        /// </summary>
        private void UpdateRendererPosition()
        {
            _renderer.Offset = _positionProp.Value.ToVector();
        }

        /// <summary>
        /// Updates the focus position.
        /// </summary>
        private void UpdateFocus()
        {
            _renderer.Focus = _focusProp.Value.ToVector();
        }

        /// <summary>
        /// Updates the reorient fov.
        /// </summary>
        private void UpdateReorient()
        {
            _renderer.ReorientFovScale = _reorientProp.Value;
        }

        /// <summary>
        /// Called when the label has been updated.
        /// </summary>
        /// <param name="prop">Alignment prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Position_OnChanged(
            ElementSchemaProp<Vec3> prop,
            Vec3 prev,
            Vec3 next)
        {
            UpdateRendererPosition();
        }

        /// <summary>
        /// Called when the focus has changed.
        /// </summary>
        /// <param name="prop">Focus prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Focus_OnChanged(
            ElementSchemaProp<Vec3> prop,
            Vec3 prev,
            Vec3 next)
        {
            UpdateFocus();
        }

        /// <summary>
        /// Called when the reorient prop changes.
        /// </summary>
        /// <param name="prop">The prop.</param>
        /// <param name="prev">Previous value.</param>
        /// <param name="next">Next value.</param>
        private void Reorient_OnChanged(
            ElementSchemaProp<float> prop,
            float prev,
            float next)
        {
            UpdateReorient();
        }
    }
}
