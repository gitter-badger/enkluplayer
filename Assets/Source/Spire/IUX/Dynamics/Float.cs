using System;
using CreateAR.Commons.Unity.Messaging;
using UnityEngine;
using Object = UnityEngine.Object;

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
        private ElementSchemaProp<Vec3> _positionProp;
        private ElementSchemaProp<Vec3> _focusProp;

        /// <summary>
        /// Constructor.
        /// </summary>
        public Float(
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

            // load font setup.
            _positionProp = Schema.GetOwn("position", new Vec3(0, 0, 2.5f));
            _positionProp.OnChanged += Position_OnChanged;
            _renderer.Offset = _positionProp.Value.ToVector();

            // focus
            _focusProp = Schema.Get<Vec3>("focus");
            _focusProp.OnChanged += Focus_OnChanged;
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
            _renderer.Focus = next.ToVector();
        }
    }
}
