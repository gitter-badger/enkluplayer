using UnityEngine;
using UnityEngine.UI;

namespace CreateAR.SpirePlayer.IUX
{
    /// <summary>
    /// Renders text.
    /// </summary>
    public class TextPrimitive
    {
        /// <summary>
        /// Pooling interface.
        /// </summary>
        private readonly IAssetPoolManager _pools;

        /// <summary>
        /// Renders text.
        /// </summary>
        private readonly Text _renderer;

        /// <summary>
        /// Parent widget.
        /// </summary>
        private Widget _parent;

        /// <summary>
        /// Bounds.
        /// </summary>
        private Rectangle _rect;

        /// <summary>
        /// Text getter/setter.
        /// </summary>
        public string Text
        {
            get
            {
                return _renderer.text;
            }
            set
            {
                _renderer.text = value;
            }
        }

        /// <summary>
        /// FontSize getter/setter.
        /// </summary>
        public int FontSize
        {
            get
            {
                return _renderer.fontSize;
            }
            set
            {
                _renderer.fontSize = value;
            }
        }

        /// <summary>
        /// Position getter/setter.
        /// </summary>
        public Vec2 Position
        {
            get
            {
                var local = _renderer.rectTransform.localPosition;

                return new Vec2(local.x, local.y);
            }
            set
            {
                var scale = _renderer.rectTransform.localScale;

                _renderer.rectTransform.localPosition = new Vector3(
                    scale.x * value.x,
                    scale.y * (value.y - _renderer.font.lineHeight / 2f),
                    _renderer.rectTransform.localPosition.z);
            }
        }

        /// <summary>
        /// Bounding rectangle.
        /// </summary>
        public Rectangle Rect
        {
            get
            {
                return _renderer.rectTransform.rect.ToRectangle();
            }
        }

        public float Width
        {
            get { return Rect.size.x; }
            set
            {
                _renderer.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Horizontal,
                    value);
            }
        }

        public float Height
        {
            get { return Rect.size.y; }
            set
            {
                _renderer.rectTransform.SetSizeWithCurrentAnchors(
                    RectTransform.Axis.Vertical,
                    value);
            }
        }

        /// <summary>
        /// Gets/sets text primitive parent.
        /// </summary>
        public Widget Parent
        {
            get
            {
                return _parent;
            }
            set
            {
                _renderer.transform.SetParent(null);
                _parent = value;

                if (null != _parent)
                {
                    _renderer.transform.SetParent(
                        _parent.GameObject.transform,
                        false);
                }
            }
        }
        
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="config">Config for widgets.</param>
        /// <param name="pools">Pooling interface.</param>
        public TextPrimitive(
            WidgetConfig config,
            IAssetPoolManager pools)
        {
            _pools = pools;

            _renderer = _pools.Get<Text>(config.Text.gameObject);
        }

        /// <summary>
        /// Destroys primitive.
        /// </summary>
        public void Destroy()
        {
            _pools.Put(_renderer.gameObject);
        }
    }
}