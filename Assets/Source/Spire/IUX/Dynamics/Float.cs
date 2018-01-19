using CreateAR.Commons.Unity.Logging;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Layout element.
    /// </summary>
    public class Float : Widget
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
        private ElementSchemaProp<Vec3> _propPosition;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Float(
            WidgetConfig config,
            IIntentionManager intention,
            IMessageRouter messages,
            ILayerManager layers,
            ITweenConfig tweens,
            IColorConfig colors)
            : base(
                new GameObject("Float"),
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

            // load font setup.
            _propPosition = Schema.GetOwn("position", new Vec3(0, 0, 2.5f));
            _propPosition.OnChanged += Position_OnChanged;
            _renderer.Offset = _propPosition.Value.ToVector();
        }

        /// <inheritdoc cref="Element"/>
        protected override void UnloadInternal()
        {
            _propPosition.OnChanged -= Position_OnChanged;

            Object.Destroy(_renderer.gameObject);

            base.UnloadInternal();
        }
        
        /// <inheritdoc />
        protected override Transform GetChildHierarchyParent(Widget child)
        {
            return _renderer.StationaryWidget.transform;
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
            _renderer.Offset = next.ToVector();
        }
    }
}
