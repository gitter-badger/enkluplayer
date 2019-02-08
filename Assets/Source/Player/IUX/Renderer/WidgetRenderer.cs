using Enklu.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.EnkluPlayer.IUX
{
    /// <summary>
    /// Updates the visual components related to a <c>Widget</c>.
    /// </summary>
    public class WidgetRenderer : InjectableMonoBehaviour
    {
        /// <summary>
        /// Cached list of materials.
        /// </summary>
        private Material[] _cachedRendererMaterials;

        /// <summary>
        /// Source widget.
        /// </summary>
        private Widget _source;

        /// <summary>
        /// Local measure of blend-in/blend out.
        /// </summary>
        private float _localTween = 1.0f;

        /// <summary>
        /// Local color.
        /// </summary>
        private Col4 _localColor = Col4.White;

        /// <summary>
        /// Visbility flag, does NOT consider parent.
        /// </summary>
        private bool _localVisible = true;

        /// <summary>
        /// Visibility flag, considers parent.
        /// </summary>
        private bool _isVisible = true;

        /// <summary>
        /// Target graphic (Unity UI rendering system).
        /// </summary>
        public Graphic Graphic;

        /// <summary>
        /// Target canvas renderer (Lower level Unity UI rendering system).
        /// </summary>
        public CanvasRenderer CanvasRenderer;

        /// <summary>
        /// Target renderer (Unity general rendering system).
        /// </summary>
        public Renderer Renderer;

        /// <summary>
        /// Target renderer (Unity general rendering system).
        /// </summary>
        public Material Material;

        /// <summary>
        /// Name of the color in the primary material of the target renderer.
        /// </summary>
        public string MaterialColorName = "_Color";

        /// <summary>
        /// Dependencies.
        /// </summary>
        [Inject]
        public TweenConfig Tweens { get; set; }

        /// <summary>
        /// Local color accessor, ignores parent color.
        /// </summary>
        public Col4 LocalColor
        {
            get { return _localColor; }
            set { _localColor = value; }
        }

        /// <summary>
        /// Local alpha.
        /// </summary>
        public float LocalAlpha { get; set; }

        /// <summary>
        /// True if locally visible, ignores parent visibility.
        /// </summary>
        public bool LocalVisible
        {
            get { return _localVisible; }
            set
            {
                _localVisible = value;

                UpdateVisibility();
            }
        }

        /// <summary>
        /// True if visible including parent visibility.
        /// </summary>
        public bool Visible
        {
            get { return _isVisible; }
        }

        /// <summary>
        /// Tween Accessor.
        /// </summary>
        public float Tween
        {
            get
            {
                var tween = _localTween;

                if (_source != null)
                {
                    tween *= _source.Tween;
                }

                return tween;
            }
        }

        /// <summary>
        /// String override.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format(
                "WidgetRenderer[{0}]",
                _source != null
                    ? _source.GameObject.name
                    : name);
        }

        /// <summary>
        /// Initializes the renderer.
        /// </summary>
        /// <param name="widget">Associated widget.</param>
        public void Initialize(Widget widget)
        {
            _source = widget;

            LocalAlpha = _source.Alpha;
        }

        /// <summary>
        /// Frame base dupdate.
        /// </summary>
        protected virtual void LateUpdate()
        {
            if (null == _source || !_source.IsLoaded)
            {
                return;
            }

            UpdateVisibility();
            UpdateTween(Time.deltaTime);

            var color = _source.Color * _localColor;
            color.a *= _localTween * LocalAlpha;

            if (Graphic != null)
            {
                Graphic.color = color.ToColor();
            }

            if (CanvasRenderer != null)
            {
                CanvasRenderer.SetAlpha(color.a);
            }

            UpdateRenderer(color);

            if (Material != null)
            {
                Material.SetColor(MaterialColorName, color.ToColor());
            }
        }

        /// <summary>
        /// Updates visibility
        /// </summary>
        private void UpdateVisibility()
        {
            _isVisible = LocalVisible && _source.Visible;
        }

        /// <summary>
        /// Updates the fade for the control
        /// </summary>
        private void UpdateTween(float deltaTime)
        {
            var tweenType = Visible
                ? _source.TweenIn
                : _source.TweenOut;

            var tweenDuration = Tweens.DurationSeconds(tweenType);
            if (tweenDuration < Mathf.Epsilon)
            {
                _localTween = Visible
                    ? 1.0f
                    : 0.0f;
            }
            else
            {
                var multiplier = Visible
                    ? 1.0f
                    : -1.0f;
                var tweenDelta = deltaTime / tweenDuration * multiplier;

                _localTween = Mathf.Clamp01(_localTween + tweenDelta);
            }
        }

        /// <summary>
        /// Updates the renderer.
        /// </summary>
        private void UpdateRenderer(Col4 color)
        {
            if (string.IsNullOrEmpty(MaterialColorName)
             || Renderer == null)
            {
                return;
            }

            if (_cachedRendererMaterials == null)
            {
                _cachedRendererMaterials = Renderer.materials;
            }

            if (_cachedRendererMaterials.Length > 0)
            {
                for (int j = 0, jCount = _cachedRendererMaterials.Length; j < jCount; ++j)
                {
                    var material = _cachedRendererMaterials[j];
                    if (material != null)
                    {
                        material.SetColor(
                            MaterialColorName,
                            color.ToColor());
                    }
                }
            }
        }
    }
}